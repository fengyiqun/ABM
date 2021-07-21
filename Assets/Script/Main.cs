using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public GameObject ROOT;
    GameObject obj = null;

    private GameObject insobj = null;
    // Start is called before the first frame update
    void Start()
    {
        string path = Application.streamingAssetsPath;
        ABM.start(path + "/MAIN.bundle");
        InitAssetToDepedencyAssets();
        while (ABM.init()) ;

       

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.A)){
          
            
        }
        if (Input.GetKey(KeyCode.C))
        {
           
        }
        if (Input.GetKey(KeyCode.D))
        {
            
        }
    }

    
    void InitAssetToDepedencyAssets()
    {
        string str = ""; 
#if UNITY_ANDROID
        StartCoroutine(DownloadFile1(ABM.getconfpath()));
#else
        str = System.IO.File.ReadAllText(ABM.getconfpath());
        var reader = new YamlDotNet.Serialization.Deserializer();
        ABM.asset_to_depedencyAssets = reader.Deserialize<Dictionary<string, List<string>>>(str);
#endif
    }


    IEnumerator DownloadFile1(string url)
    {
        UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(url);
        request.timeout = 10;
        yield return request.SendWebRequest();
        if (request.error != null)
        {
            Debug.LogErrorFormat("加载出错： {0}, url is: {1}", request.error, url);
            request.Dispose();
            yield break;
        }

        if (request.downloadHandler.isDone)
        {
            string str = request.downloadHandler.text;
            var reader = new YamlDotNet.Serialization.Deserializer();
            ABM.asset_to_depedencyAssets = reader.Deserialize<Dictionary<string, List<string>>>(str);
            yield break;
        }
    }

    public void LoadAB()
    {
        obj = ABM.load_assetnew("assets/art/b.prefab") as GameObject;
    }
    public void LoadDepedency()
    {
        ABM.load_depedencyAssets("assets/art/b.prefab");
    }

    public void LoadAsset()
    {
        if (obj != null)
        {
            insobj = GameObject.Instantiate(obj);
            insobj.transform.SetParent(ROOT.transform);
        }
    }

    public void DeleteAsset()
    {
        GameObject.DestroyImmediate(obj,true);
    }

    public void UnLoadAsset()
    {

        GameObject.Destroy(insobj);
        if (obj != null)
        {
            ABM.unload_assetnew(obj);
            obj = null;
        }
    }

    public void UnLoadAB()
    {
        ABM.unLoad_assetbundle();
    }

    public void UnloadUnusedAssets()
    {
        ABM.unload_unusedassets();
    }
}
