using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxServer_dll.MessageHandlers
{
    public class LoginHandler : MessageHandler
    {
        public override void Handle()
        {
            string label = GetStringValue("label");
            if (string.IsNullOrEmpty(label))
            {
                Debug.Error("login信息没有添加label");
                return;
            }
            Response res;
            LiveClientSide saved = server.GetClientBySocket(client);
            if (saved != null)
            {
                res = new Response(true, "客户端已存在");
                res.AddProperty("action", "login");
                res.AddProperty("id", saved.ID);
                server.Send(res.Create(), client);
                return;
            }
            LiveClientSide clientSide = new LiveClientSide(client, label);
            server.AddClient(clientSide);
            res = new Response(true, "登录成功");
            res.AddProperty("action", "login");
            res.AddProperty("id", clientSide.ID);
            server.Send(res.Create(), client);
            Debug.Log(string.Format("添加了客户端:{0}:{1}", clientSide.Label, clientSide.ID));
        }
        public LoginHandler(Dictionary<string,string> dataForm) : base(dataForm)
        {

        }
    }
}
