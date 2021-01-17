using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxServer_dll
{
    public class MessageHandlerBase
    {
        public string Label;

        public readonly TCPMessage Message;
        virtual public void Handle()
        {

        }
        public MessageHandlerBase(TCPMessage message)
        {

        }
    }
}
