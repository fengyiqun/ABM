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
        var path = "ProjectSettings/ABT.yaml";
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

}
