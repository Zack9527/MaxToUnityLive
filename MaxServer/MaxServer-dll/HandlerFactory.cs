using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxServer_dll
{
    public class HandlerFactory
    {

        public static MessageHandlerBase Produce(TCPMessage message)
        {
            MessageHandlerBase handler = null;
            string cmd = message.Message.ToLower();
            switch (cmd)
            {
                case "login":
                    handler = new LoginHandler(message);
                    break;
                default:
                    handler = new NoHandler(message);
                    break;
            }
            return handler;
        }
    }

    public class NoHandler : MessageHandlerBase
    {
        public NoHandler(TCPMessage message) : base(message)
        {

        }
    }
    public class LoginHandler : MessageHandlerBase
    {
        public override void Handle()
        {
            base.Handle();
        }
        public LoginHandler(TCPMessage message) : base(message)
        {

        }
    }

    
}
