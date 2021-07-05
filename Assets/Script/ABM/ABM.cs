using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ABM 
{
    private class ABO
    {
        public int abi = -1;
        public int refn = 1;
        public bool isLoad = false;
        public AssetBundle ab = null;
        public HashSet<int> dependencies = new HashSet<int>();
    }
    private class AO
    {
        public string name = null;
        public int abi = -1;
        public int refn = 1;
        public object asset = null;
        public AO(int abi,string name)
        {
            this.abi = abi;
            this.name = name;
        }
    }
    private class initctx
    {
        public int i;
        public string[] files;
    }
    private enum OP
    {
        SET_ACTIVE_SCENE = 1,
    }
    private class operation
    {
        public OP op;
        public string ud;
        public operation(OP op,string ud)
        {
            this.op = op;
            this.ud = ud;
        }
    }
    private static int ABIDX = 0;
    private static string ROOT = null;
    private static string MAINNAME = "MAIN.bundle";
    private static AssetBundleManifest MANIFEST = null;
    private static initctx ctx = null;
    private static Dictionary<int, string> abi_to_name = new Dictionary<int, string>();
    private static Dictionary<string, int> name_to_abi = new Dictionary<string, int>();
    private static Dictionary<int, ABO> abi_to_abo = new Dictionary<int, ABO>();
    private static Dictionary<int, ABO> abi_to_abo_unload = new Dictionary<int, ABO>();
    private static Dictionary<string,int> abi_of_asset = new Dictionary<string, int>();
    private static Dictionary<string,AO> name_to_ao = new Dictionary<string, AO>();
    private static Dictionary<int,AO> object_to_ao = new Dictionary<int, AO>();
    private static List<operation> operations_currentframe = new List<operation>();
    private static List<operation> operation_nextframe = new List<operation>();
    private static Dictionary<string, string[]> abo_to_loadnamt = new Dictionary<string, string[]>();
    private static Dictionary<string, List<string>> asset_to_depedencyAssets = new Dictionary<string, List<string>>();
    static string getconfpath()
    {
        var path = Application.streamingAssetsPath + "/ABT.yaml";
        Debug.Log("ABT configPaht:" + path);
        return path;
    }

    private static void DEBUGPRINT(string str)
    {
        UnityEngine.Debug.Log(str);
    }

    private static ABO load_abo(int abi,List<int> abistack)
    {
        abistack.Add(abi);
        ABO abo = null;
        abi_to_abo.TryGetValue(abi, out abo);
        if(abo != null)
        {
            abo.refn++;
            DEBUGPRINT("load_abo abi:" + abi + " ref:" + abo.refn);
            return abo;
        }
        abo = new ABO();
        abi_to_abo[abi] = abo;
        var name = abi_to_name[abi];
        var depend = MANIFEST.GetDirectDependencies(name);
        for (int i = 0; i < depend.Length; i++)
        {
            var dn = depend[i];
            var di = 0;
            name_to_abi.TryGetValue(dn, out di);
            if (di == 0)
            {
                DEBUGPRINT("load_abo depend nonexist assetbundle:"+dn);
                return null;
            }
            if (!abistack.Contains(di))
            {
                load_abo(di, abistack);
                abo.dependencies.Add(di);
            }
            
        }

        var path = Path.Combine(ROOT, name);
        DEBUGPRINT("load_abo depend assetbundle:"+path);
        var ab = AssetBundle.LoadFromFile(path);
        System.Diagnostics.Debug.Assert(ab != null);
        abo.ab = ab;
        abo.abi = abi;
        return abo;
    }

    private static ABO load_abonew(int abi, List<int> abistack)
    {
        abistack.Add(abi);
        ABO abo = null;
        abi_to_abo.TryGetValue(abi, out abo);
        if (abo != null)
        {
            abo.refn++;
            DEBUGPRINT("load_abo abi:" + abi + " ref:" + abo.refn);
            return abo;
        }
        abi_to_abo_unload.TryGetValue(abi, out abo);
        if(abo != null)
        {
            abo.refn++;
            abi_to_abo_unload.Remove(abi);
            DEBUGPRINT("load_abo back abi:" + abi + " ref:" + abo.refn);
            return abo;
        }
        abo = new ABO();
        abi_to_abo[abi] = abo;
        var name = abi_to_name[abi];
        var path = Path.Combine(ROOT, name);
        DEBUGPRINT("load_abo depend assetbundle:" + path);
        var ab = AssetBundle.LoadFromFile(path);
        System.Diagnostics.Debug.Assert(ab != null);
        abo.isLoad = true;
        abo.ab = ab;
        abo.abi = abi;
        return abo;
    }
    private static void unload_abo(int abi)
    {
        ABO abo = null;
        abi_to_abo.TryGetValue(abi, out abo);
        if (abo == null)
        {
            DEBUGPRINT("unload_abo nonexist abo"+abi);
            return;
        }

        --abo.refn;
        DEBUGPRINT("unload_abo abi:"+abi+" ref:"+abo.refn);
        if (abo.refn <= 0)
        {
            var iter = abo.dependencies.GetEnumerator();
            while (iter.MoveNext())
            {
                unload_abo(iter.Current);
            }
            abo.ab.Unload(true);
            abi_to_abo.Remove(abi);
            DEBUGPRINT("unload_abo clear abi:"+abi);
        }
    }
    private static void unload_abonew(int abi)
    {
        ABO abo = null;
        abi_to_abo.TryGetValue(abi, out abo);
        if (abo == null)
        {
            DEBUGPRINT("unload_abo nonexist abo" + abi);
            return;
        }
        bool unload = true;
        foreach(var value in abi_of_asset)
        {
            if(value.Value == abi)
            {
                AO ao = null;
                if(name_to_ao.TryGetValue(value.Key,out ao))
                {
                    if(ao.refn > 0)
                    {
                        unload = false;
                    }
                }
            }
        }
        DEBUGPRINT("unload_abo abi:" + abi + " ref:" + abo.refn);
        if (unload)
        {
            abi_to_abo_unload[abi] = abo;
            abi_to_abo.Remove(abi);
            DEBUGPRINT("unload_abo clear abi:" + abi);
        }
    }
   
    public static int start(string path)
    {
        ctx = new initctx();
        var c = path[path.Length - 1];
        var ab = AssetBundle.LoadFromFile(Path.Combine(path, MAINNAME));
        MANIFEST = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        ab.Unload(false);
        ROOT = path;
        ctx.i = 0;
        ctx.files = MANIFEST.GetAllAssetBundles();
        InitAssetToDepedencyAssets();
        return ctx.files.Length;
    }

    static void InitAssetToDepedencyAssets()
    {
        var str = System.IO.File.ReadAllText(getconfpath());
        var reader = new YamlDotNet.Serialization.Deserializer();
        asset_to_depedencyAssets = reader.Deserialize<Dictionary<string, List<string>>>(str);
    }

    public static bool init()
    {
        if (ctx.i < ctx.files.Length)
        {
            var abi = ++ABIDX;
            var subname = ctx.files[ctx.i];
            var name = Path.Combine(ROOT, subname);
            var ab = AssetBundle.LoadFromFile(name);
            var asset_child = ab.GetAllAssetNames();
            var scene_child = ab.GetAllScenePaths();
            abi_to_name[abi] = subname;
            name_to_abi[subname] = abi;
            for (int i = 0; i < asset_child.Length; i++)
            {
                var s = asset_child[i];
                DEBUGPRINT("Asset:" + s + " -> " + subname);
                abi_of_asset [s] = abi;
            }

            for (int i = 0; i < scene_child.Length; i++)
            {
                var s = scene_child[i];
                DEBUGPRINT("Scene:" + s + " ->" + subname);
                abi_of_asset [s] = abi;
            }
            ab.Unload(true);
            ctx.i++;
            return true;
        }
        else
        {
            ctx = null;
            return false;
        }
    }



    public static T load_asset<T>(string name_) where T : Object
    {
        int abi = 0;
        AO ao = null;
        var name = name_.ToLower();
        DEBUGPRINT("ABM:load_asset:"+name);
        if (name_to_ao.TryGetValue(name_, out ao))
        {
            DEBUGPRINT("load_asset name:"+name+" hit");
            ++ao.refn;
            return (T)ao.asset;
        }

        if (abi_of_asset .TryGetValue(name, out abi) == false)
            return null;
        var abo = load_abo(abi,new List<int>());
        if (abo == null)
            return null;
        var asset = abo.ab.LoadAsset<T>(name);
        if (asset != null)
        {
            ao = new AO(abi,name);
            ao.asset = asset;
            name_to_ao[name] = ao;
            object_to_ao[asset.GetInstanceID()] = ao;
        }
        return asset;
    }

     static void load_depedencyAssets(string name_)
    {
        

        List<string> depedencyAssets = new List<string>();
        if(asset_to_depedencyAssets.TryGetValue(name_,out depedencyAssets) == false)
        {
            DEBUGPRINT("loaddepedencyAssets Fail asset = " + name_);
            return;
        }
        for(int i  = 0; i < depedencyAssets.Count; i++)
        {
            var name = depedencyAssets[i].ToLower();
            int abi = 0;
            AO ao = null;
            if(name_to_ao.TryGetValue(name,out ao))
            {
                DEBUGPRINT("load_asset name:" + name + " hit");
                ++ao.refn;
            }
            if(abi_of_asset.TryGetValue(name,out abi) == false)
            {
                DEBUGPRINT("Find abi Fail asset = " + name);
                return;
            }
            var abo = load_abo(abi, new List<int>());
            if(abo == null)
            {
                DEBUGPRINT("load abo is Fail asset : " + name);
                return;
            }
            var asset = abo.ab.LoadAsset(name);
            if(asset != null)
            {
                ao = new AO(abi, name);
                ao.asset = asset;
                name_to_ao[name] = ao;
                object_to_ao[asset.GetInstanceID()] = ao;
            }
        }

    }

    public static T load_assetNew<T>(string name_) where T : Object
    {
        int abi = 0;
        AO ao = null;
        var name = name_.ToLower();
        DEBUGPRINT("ABM:load_asset:" + name);
        if(name_to_ao.TryGetValue(name_,out ao))
        {
            DEBUGPRINT("load_asset name:" + name + " hit");
            ++ao.refn;
            return (T)ao.asset;
        }
        if (abi_of_asset.TryGetValue(name, out abi) == false)
            return null;
        var abo = load_abonew (abi, new List<int>());
        if (abo == null)
            return null;
        load_depedencyAssets(name);
        var asset = abo.ab.LoadAsset<T>(name);
        if (asset != null)
        {
            ao = new AO(abi, name);
            ao.asset = asset;
            name_to_ao[name] = ao;
            object_to_ao[asset.GetInstanceID()] = ao;
        }
        return asset;

    }




    public static void ref_asset(int instance_id)
    {
        AO ao = null;
        if (object_to_ao.TryGetValue(instance_id, out ao))
        {
            ++ao.refn;
            DEBUGPRINT("ref_asset instance id:"+instance_id+"ref count:"+ao.refn);
        }
        System.Diagnostics.Debug.Assert(ao!= null);
        return;
    }

    static void unload_asset(AO ao)
    {
        Resources.UnloadAsset((Object)ao.asset);
    }

    

    public static void unref_assetnew(int instance_id)
    {
        AO ao = null;
        if (object_to_ao.TryGetValue(instance_id, out ao) == false)
            return;
        --ao.refn;
        DEBUGPRINT("unref_asset instance id:" + instance_id + "ref count:" + ao.refn);
        if (ao.refn <= 0)
        {
            unload_asset(ao);
            unload_abonew(ao.abi);
            name_to_ao.Remove(ao.name);
            object_to_ao.Remove(instance_id);
        }
        return;
    }

    public static void unref_asset(int instance_id)
    {
        AO ao = null;
        if(object_to_ao.TryGetValue(instance_id,out ao) == false)
            return;
        --ao.refn;
        DEBUGPRINT("unref_asset instance id:"+instance_id+"ref count:"+ao.refn);
        if (ao.refn <= 0)
        {
            unload_abo(ao.abi);
            name_to_ao.Remove(ao.name);
            object_to_ao.Remove(instance_id);
        }
        return;
    }

    public static Object load_asset(string name)
    {
        if (name.EndsWith(".png") || name.EndsWith(".jpg"))
        {
            return load_asset<Sprite>(name);
        }
        else
        {
            return load_asset<Object>(name);
        }
    }

    public static void unload_asset(Object obj)
    {
        unref_asset(obj.GetInstanceID());
    }

    public static void unLoad_assetbundle()
    {
        foreach(var value in abi_to_abo_unload)
        {
            value.Value.ab.Unload(true);
            value.Value.isLoad = false;
        }
        abi_to_abo_unload.Clear();
    }

    public static void load_scene(string name, LoadSceneMode mode)
    {
        int abi;
        if (abi_of_asset .TryGetValue(name, out abi) == false)
        {
            DEBUGPRINT("load_scene:" + name + " fail");
            return;
        }

        var abo = load_abo(abi,new List<int>());
        if (abo == null)
        {
            DEBUGPRINT("load_scene:"+name+" fail");
            return;
        }

        var scenename = Path.GetFileNameWithoutExtension(name);
        SceneManager.LoadScene(scenename, mode);
        DEBUGPRINT("load_scene"+scenename);
        return;
    }

    public static AsyncOperation load_scene_async(string name, LoadSceneMode mode)
    {
        int abi;
        if (abi_of_asset .TryGetValue(name, out abi) == false)
        {
            return null;
        }

        var abo = load_abo(abi,new List<int>());
        if (abo == null)
        {
            return null;
        }

        var scenename = Path.ChangeExtension(name, null);
        return SceneManager.LoadSceneAsync(scenename, mode);
    }

    public static AsyncOperation unload_scene_async(string name)
    {
        int abi;
        if (abi_of_asset.TryGetValue(name, out abi) == false)
        {
            return null;
        }
        unload_abo(abi);
        return SceneManager.UnloadSceneAsync(name);
    }

    public static void set_active_scene(string scenepath)
    {
        var scenename = Path.GetFileNameWithoutExtension(scenepath);
        operation_nextframe.Add(new operation(OP.SET_ACTIVE_SCENE,scenename));
    }

    public static void update()
    {
        var exec = operations_currentframe;
        operations_currentframe = operation_nextframe;
        var iter = exec.GetEnumerator();
        while (iter.MoveNext())
        {
            var op = iter.Current;
            switch (op.op)
            {
                case OP.SET_ACTIVE_SCENE:
                    var scene = SceneManager.GetSceneByName(op.ud);
                    DEBUGPRINT("SetActivityScene:"+op.ud+":"+scene);
                    SceneManager.SetActiveScene(scene);
                    break;
            }
        }
        exec.Clear();
        operation_nextframe = exec;
    }
}
