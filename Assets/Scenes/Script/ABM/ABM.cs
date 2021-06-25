using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ABM 
{
    private class ABO
    {
        public int abi = -1;
        public int refn = 1;
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

    private static void DEBUGPRINT(string str)
    {

    }

    private static ABO load_abo(int abi)
    {
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

        return abo;
    }
}
