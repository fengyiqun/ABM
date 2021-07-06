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

    public void LoadAB()
    {
        obj = ABM.load_assetnew("assets/art/b.prefab") as GameObject;
    }

    public void LoadAsset()
    {
        if (obj != null)
        {
            insobj = GameObject.Instantiate(obj);
            insobj.transform.SetParent(ROOT.transform);
        }
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
