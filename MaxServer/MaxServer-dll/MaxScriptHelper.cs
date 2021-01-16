using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;

namespace MaxServer_dll
{
    public class MaxScriptHelper
    {
        public static IGlobal g = null;
        public static void OTL(string message)
        {
            g.TheListener.EditStream.Printf(message + "\n");
        }

        internal static void ExecuteMaxCommondStr(string str)
        {
            //Mana
        }
        internal static void Initialze()
        {
            g = GlobalInterface.Instance;
        }

        internal static void LaunchMaxScript(string path)
        {
            g.FileinScript(path);
        }
    }
}
