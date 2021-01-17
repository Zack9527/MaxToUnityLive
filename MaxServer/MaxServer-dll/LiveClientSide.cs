using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MaxServer_dll
{
    public class LiveClientSide
    {
        public readonly Socket Client;
        public readonly string Label;
        public readonly string ID;
        public LiveClientSide(Socket socket, string label)
        {
            this.Client = socket;
            this.Label = label;
            ID = GetGuid();
        }
        private string GetGuid()
        {
            Guid guid = new Guid();
            guid = Guid.NewGuid();
            return guid.ToString();
        }
}
}
