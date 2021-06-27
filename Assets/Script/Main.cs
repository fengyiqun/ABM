using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public GameObject ROOT;
    GameObject obj = null;
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
            obj = ABM.load_asset("assets/art/c.prefab") as GameObject;
            
        }
        if (Input.GetKey(KeyCode.C))
        {
            GameObject go = GameObject.Instantiate(obj);
            go.transform.SetParent(ROOT.transform);
        }
        if (Input.GetKey(KeyCode.D))
        {
            ABM.unload_asset(obj);
        }
    }
}
