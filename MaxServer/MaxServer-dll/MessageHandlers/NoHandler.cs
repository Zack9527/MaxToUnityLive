using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxServer_dll.MessageHandlers
{
    public class NoHandler : MessageHandler
    {
        public NoHandler(Dictionary<string, string> dataForm) : base(dataForm)
        {

        }
        public override void Handle()
        {
            Response res = new Response(false, "no action");
            server.Send(res.Create(), client);
        }
    }
}
