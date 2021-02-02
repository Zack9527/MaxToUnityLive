using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxServer_dll.MessageHandlers
{
    public class StateHandler: MessageHandler
    {
        public StateHandler(Dictionary<string, string> dataForm) : base(dataForm)
        {

        }
        public override void Handle()
        {
            string cmd = GetStringValue("cmd");
            Response res = null;
            if (string.IsNullOrEmpty(cmd))
            {
                res = new Response(false, "no cmd");
                server.Send(res.Create(), client);
                return;
            }
            cmd = cmd.ToLower();
            switch (cmd)
            {
                case "clientlist":
                    res = new Response(true, "succeed");
                    int count = 0;
                    foreach(var client in server.clients)
                    {
                        res.AddProperty("client" + count, client.ID);
                        count++;
                    }
                    break;
                default:
                    res = new Response(false, "no such cmd");
                    break;
            }
            server.Send(res.Create(), client);
        }
    }
}
