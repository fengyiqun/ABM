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
    static Dictionary<string,int>name_to_asset = new Dictionary<string, int>();
    static Dictionary<string,AO> name_to_ao = new Dictionary<string, AO>();
    static Dictionary<int ,OBJ> obji_to_obj = new Dictionary<int, OBJ>();
    static Dictionary<string,List<string>> asset_to_depedencyAssets = new Dictionary<string, List<string>>();
    List<AssetBundleRequest > reqlistloading = new List<AssetBundleRequest>();

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
        abo.isLoad = loadType.loading;
        abo.abi = abi;
        abo.ab = null;
        return abo;
    }

    static void unload_abo(int abi)
    {
        
    }
}