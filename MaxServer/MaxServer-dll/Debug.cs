using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxServer_dll
{
    public class Debug
    {
        public static void Log(object msg)
        {
            Console.WriteLine(msg.ToString());
        }

        public static void Warning(object msg)
        {
            Console.WriteLine(msg.ToString());
        }

        public static void Error(object msg)
        {
            Console.WriteLine(msg.ToString());
        }
    }
}
