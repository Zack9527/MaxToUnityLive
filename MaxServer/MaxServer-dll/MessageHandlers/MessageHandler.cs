using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MaxServer_dll
{
    public class MessageHandler
    {
        public Server server;
        public LiveClientSide clientSide;
        public Socket client;
        public readonly Dictionary<string, string> DataForm;

        virtual public void Handle()
        {

        }
        public MessageHandler(Dictionary<string,string> dataForm)
        {
            DataForm = dataForm;
        }
        public string GetStringValue(string key)
        {
            if (DataForm.ContainsKey(key))
                return DataForm[key];
            return string.Empty;
        }

    }
}
