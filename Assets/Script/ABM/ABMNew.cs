using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class ABMNew
{
    public delegate void LoadAssetSuccessCallback(string assetName, Object asset, float duration, object userData);

    public delegate void LoadAssetFailureCallback(string assetName, string errorMessage, object userData);
    enum loadType
    {
        none = 0,
        loading = 1,
        load = 2,
        unload = 3,
    }
    class  ABO
    {
        public int abi = -1;
        public loadType isLoad = loadType.none;
        public AssetBundle ab = null;
        public HashSet<int> objs = new HashSet<int>();
    }

    class AO
    {
        public string name = null;
        public int abi = -1;
        public int refn = 1;
        public Object asset = null;
        
        public AO(int abi, string name)
        {
            this.abi = abi;
            this.name = name;
        }
    }

    class OBJ
    {
        private LoadAssetFailureCallback m_LoadAssetFailureCallback;
        private LoadAssetSuccessCallback m_LoadAssetSuccessCallback;
        List<int> aboList = new List<int>();
        private AO ao = null;
        private int obji = -1;
        private string name;
        public int refn = 1;
        public OBJ(string name, int index, LoadAssetSuccessCallback scb, LoadAssetFailureCallback fcb)
        {
            this.name = name;
            this.obji = index;
            m_LoadAssetSuccessCallback = scb;
            m_LoadAssetFailureCallback = fcb;
        }

        public AO Ao
        {
            get { return ao; }
            set { ao = value; }
        }

        public string getName
        {
            get { return name; }
        }

        public int getIndex
        {
            get { return obji; }
        }
    }

    class initctx
    {
        public int i;
        public string[] files;
    }

    private static int ABIDX = 0;
    private static int OBJIDX = 0;
    private static string ROOT = null;
    private static string MAINNAME = "MAIN.bundle";
    private static AssetBundleManifest MANIFEST = null;
    private static initctx ctx = null;
    static Dictionary<int,string>abi_to_name = new Dictionary<int, string>();
    static Dictionary<string,int>name_to_abi = new Dictionary<string, int>();
    static Dictionary<int,ABO> abi_to_abo = new Dictionary<int, ABO>();
    static Dictionary<string,int>abi_of_asset = new Dictionary<string, int>();
    static Dictionary<string,AO> name_to_ao = new Dictionary<string, AO>();
    static Dictionary<int ,OBJ> obji_to_obj = new Dictionary<int, OBJ>();
    static Dictionary<string,OBJ> name_to_obj = new Dictionary<string, OBJ>();
    static Dictionary<int, AssetBundleCreateRequest> abi_to_request = new Dictionary<int, AssetBundleCreateRequest>();
    static Dictionary<string,List<string>> asset_to_depedencyAssets = new Dictionary<string, List<string>>();
    static Dictionary<string,AssetBundleRequest > reqlistloadingDic = new Dictionary<string, AssetBundleRequest>();
    
    public static string getconfpath()
    {
        var path = "";
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
        path =  Application.dataPath + "/StreamingAssets/ABT.yaml";
#elif UNITY_ANDROID
        path = "jar:file://" + Application.dataPath + "!/assets/ABT.yaml";
#elif UNITY_IOS
         path =  Application.dataPath + "/Raw/ABT.yaml";
#endif
        return path;
    }

    static void DEBUGPRINT(string str)
    {
        Debug.Log(str);
    }

    static ABO load_abo(int abi)
    {
        ABO abo = null;
        abi_to_abo.TryGetValue(abi, out abo);
        if (abo != null)
        {
            DEBUGPRINT("load_abo abi:"+abi);
            return abo;
        }
        abo = new ABO();
        abi_to_abo[abi] = abo;
        var name = abi_to_name[abi];
        var path = Path.Combine(ROOT, name);
        DEBUGPRINT("load_abo assetbundle path :" + path);
        var req = AssetBundle.LoadFromFileAsync(path);
        abi_to_request[abi] = req;
        abo.isLoad = loadType.loading;
        abo.abi = abi;
        abo.ab = null;
        return abo;
    }

    static void unload_abo(int abi)
    {
        ABO abo = null;
        abi_to_abo.TryGetValue(abi, out abo);
        if(abo == null)
        {
            DEBUGPRINT("unload_abo nonexist abo: " + abi);
            return;
        }
        bool unload = true;
        if(abo.isLoad == loadType.load)
        {
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
        }
        DEBUGPRINT("unload_abo abi:" + abi + " unload: " + unload);
        if (unload)
        {
            abo.isLoad = loadType.unload;
            DEBUGPRINT("unload_abo clear abi :" + abi);
        }
    }
    public static int start(string path)
    {
        ctx = new initctx();
        var ab = AssetBundle.LoadFromFile(Path.Combine(path, MAINNAME));
        MANIFEST = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        ab.Unload(false);
        ROOT = path;
        ctx.i = 0;
        ctx.files = MANIFEST.GetAllAssetBundles();
        return ctx.files.Length;
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
            var scene_chind = ab.GetAllScenePaths();
            abi_to_name[abi] = subname;
            name_to_abi[subname] = abi;
            for(int i = 0; i < asset_child.Length; i++)
            {
                var s = asset_child[i];
                DEBUGPRINT("Asset:" + s + " ->" + subname);
                abi_of_asset[s] = abi;
            }
            for(int i  = 0; i < scene_chind.Length; i++)
            {
                var s = scene_chind[i];
                DEBUGPRINT("Scene: " + s + " ->" + subname);
                abi_of_asset[s] = abi;
            }
            ab.Unload(true);
            ctx.i++;
            return true;
        }
        else {
            ctx = null;
            return false;
        }
    }
    static void load_depedencyAssets(string name_,List<int> abilist)
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
                ++ao.refn;
                DEBUGPRINT("load_asset name:" + name + "refn" + ao.refn);
                return;
            }
            if(abi_of_asset.TryGetValue(name,out abi) == false)
            {
                DEBUGPRINT("load abi is fail asset: " + name);
            }
            var abo = load_abo(abi);
            if(abo == null)
            {
                DEBUGPRINT("load abo is Fail asset:" + name);
                return;
            }
            if (!abilist.Contains(abi))
            {
                abilist.Add(abi);
            }
            
            load_depedencyAssets(name,abilist);
            ao = new AO(abi, name);
            ao.asset = null;
            name_to_ao[name] = ao;
        }
    }
    public static T load_asset<T>(string name_,List<int> abilist) where T : Object
    {
        int abi = 0;
        AO ao = null;
        var name = name_.ToLower();
        //if(name_to_ao.TryGetValue(name_))
        if(name_to_ao.TryGetValue(name,out ao))
        {
            DEBUGPRINT("load_asset name:"+name+"hit");
            ++ao.refn;
            if (ao.asset != null)
            {
                return (T) ao.asset;
            }
            return null;
        }

        if (abi_of_asset.TryGetValue(name, out abi) == false)
        {
            return null;
        }

        var abo = load_abo(abi);
        abilist.Add(abi);
        if (abo == null)
            return null;
        load_depedencyAssets(name,abilist);
        if (abo.ab != null)
        {
            abo.isLoad = loadType.load;
        }
        ao = new AO(abi,name);
        name_to_ao[name] = ao;
        if (abo.isLoad == loadType.load)
        {
            AssetBundleRequest abreq = abo.ab.LoadAssetAsync(name);
            reqlistloadingDic.Add(name,abreq);
        }
        return (T)ao.asset;
    }

    static void unload_asset(AO ao)
    {
        DEBUGPRINT("Unloadasset Name: "+ao.name);
        if (ao.asset != null)
        {
            Resources.UnloadAsset((Object)ao.asset);
        }
    }

    static void unref_assetdepedency(AO ao)
    {
        List<string> depedencylist = new List<string>();
        if (asset_to_depedencyAssets.TryGetValue(ao.name, out depedencylist))
        {
            AO depedencyao = null;
            for (int i = 0; i < depedencylist.Count; i++)
            {
                if (name_to_ao.TryGetValue(depedencylist[i], out depedencyao))
                {
                    --depedencyao.refn;
                    if (depedencyao.refn <= 0)
                    {
                        unload_abo(ao.abi);
                        unref_assetdepedency(depedencyao);
                        name_to_ao.Remove(ao.name);
                    }
                }
            }
        }
    }

    public static void unref_asset(int instance_id)
    {
        OBJ obj = null;
        if(obji_to_obj.TryGetValue(instance_id,out obj) == false)
            return;
        --obj.Ao.refn;
        DEBUGPRINT("unref_asset instance id"+instance_id + "ref count:"+ obj.Ao.refn);
        if (obj.Ao.refn <= 0)
        {
            unref_assetdepedency(obj.Ao);
            unload_abo(obj.Ao.abi);
            name_to_ao.Remove(obj.Ao.name);
            obji_to_obj.Remove(instance_id);
        }
    }

    static Object load_obj(string name)
    {
        OBJ obj = null;
        //if(obji_to_obj.TryGetValue())
    }
}