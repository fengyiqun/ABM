using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ABT : UnityEditor.EditorWindow
{
    static bool FORCEREBUILD = false;
    static string ENTRY = "MAIN.bundle";
    static string OUTPUT_PATH = Application.streamingAssetsPath;
    static string PROJECT_PATH = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../"));
    class AssetBundleInfo
    {
        public string Entry { get; set; }
        public Dictionary<string, string[]> AssetBundles { get; set; }
        public AssetBundleInfo()
        {
            Entry = ENTRY;
            AssetBundles = new Dictionary<string, string[]>();
        }
    }
    static string getconfpath()
    {
        var path = OUTPUT_PATH+"/ABT.yaml";
        Debug.Log("ABT configPaht:" + path);
        return path;
    }
    static void check_modified(string output_root)
    {
        var path = getconfpath();
        if(System.IO.File.Exists(path) == false)
        {
            try
            {
                System.IO.Directory.Delete(output_root, true);
            }
            catch (System.Exception)
            {
               
            }
            return;
        }
        int prefix = output_root.Length;
        int c = output_root[output_root.Length - 1];
        if(c!= '\\' && c!= '/')
        {
            prefix += 1;
        }
        var str = System.IO.File.ReadAllText(getconfpath());
        var reader = new YamlDotNet.Serialization.Deserializer();
        var abm = reader.Deserialize<AssetBundleInfo>(str);
        var bundle_list = System.IO.Directory.GetFiles(output_root, "*");
        foreach(var bundle_path in bundle_list)
        {

        }
    }

    static void build_target(UnityEditor.BuildTarget target)
    {
        string output = System.IO.Path.Combine(OUTPUT_PATH, ENTRY);
        UnityEditor.BuildAssetBundleOptions option = UnityEditor.BuildAssetBundleOptions.DeterministicAssetBundle;
        option |= UnityEditor.BuildAssetBundleOptions.StrictMode;
        option |= UnityEditor.BuildAssetBundleOptions.ChunkBasedCompression;
        option |= UnityEditor.BuildAssetBundleOptions.DisableLoadAssetByFileName;
        option |= UnityEditor.BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;
        if(FORCEREBUILD == true)
        {
            option |= UnityEditor.BuildAssetBundleOptions.ForceRebuildAssetBundle;
            try
            {
                System.IO.Directory.Delete(output, true);
            }
            catch (System.Exception)
            {

            }
        }
        else
        {
            check_modified(output);
        }
        System.IO.Directory.CreateDirectory(output);
        UnityEditor.BuildPipeline.BuildAssetBundles(output, option, target);
        UnityEditor.AssetDatabase.Refresh();
        Debug.Log("ABT build finish: " + output);
    }
    [UnityEditor.MenuItem("ABT/Build AssetBundle(Mac)")]
    static void build_mac() {
        build_target(UnityEditor.BuildTarget.StandaloneOSX);
    }
    [UnityEditor.MenuItem("ABT/Build AssetBundle(PC)")]
    static void build_pc()
    {
        build_target(UnityEditor.BuildTarget.StandaloneWindows64);
    }
    static Dictionary<string, List<string>> depedencyAssets = new Dictionary<string, List<string>>();

    static void AnalyzAsset(string assetName,HashSet<string> scriptAssetNames)
    {
        string[] depedencyAssetNames = UnityEditor.AssetDatabase.GetDependencies(assetName, false);
        foreach(string dependencyAssetName in depedencyAssetNames)
        {
            if (scriptAssetNames != null && scriptAssetNames.Contains(dependencyAssetName))
            {
                continue;
            }
            if(dependencyAssetName == assetName)
            {
                continue;
            }
            if (dependencyAssetName.EndsWith(".unity", System.StringComparison.Ordinal))
            {
                continue;
            }
            if (depedencyAssets.ContainsKey(assetName))
            {
                depedencyAssets[assetName].Add(dependencyAssetName);
            }
            else
            {
                depedencyAssets[assetName] = new List<string>() { dependencyAssetName };
            }
        }
    }

    class AssetDepedency
    {
        public string AssetName { get; set; }
        public List<string> AssetDepedencyList { get; set; }
    }
    [UnityEditor.MenuItem("ABT/SelectAssetDepedencies")]
    static void SelectAssetDependencies()
    {
        depedencyAssets.Clear();
        string[] datas = UnityEditor.AssetDatabase.FindAssets("t:Prefab,t:material");
        foreach(string o in datas)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(o);
            Debug.Log(UnityEditor.AssetDatabase.GUIDToAssetPath( o));
            AnalyzAsset(path,null);
        }
        var writer = new YamlDotNet.Serialization.Serializer();
        string str = writer.Serialize(depedencyAssets);
        Debug.Log(str);
        System.IO.File.WriteAllText(getconfpath(), str);
    }
}
