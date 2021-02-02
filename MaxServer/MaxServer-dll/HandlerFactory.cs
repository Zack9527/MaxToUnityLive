using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MaxServer_dll
{
    public class HandlerFactory
    {
        public static MessageHandler Produce(Dictionary<string,string> dataForm, Socket client, Server server)
        {
            MessageHandler handler = null;
            string act = string.Empty;
            if (dataForm.ContainsKey("action"))
                act = dataForm["action"];
            switch (act)
            {
                case "login":
                    handler = new MessageHandlers.LoginHandler(dataForm);
                    break;
                case "state":
                    handler = new MessageHandlers.StateHandler(dataForm);
                    break;
                default:
                    handler = new MessageHandlers.NoHandler(dataForm);
                    break;
            }
            handler.client = client;
            handler.server = server;
            return handler;
        }
    }

    
}
