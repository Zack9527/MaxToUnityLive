using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxServer_dll
{
    public class Response
    {
        private JsonBuilder builder;
        public Response(bool succeed,string message)
        {
            builder = JsonHelper.CreateJsonObjectBuilder();
            builder.SetProperty("reslut", succeed);
            builder.SetProperty("message", message);
        }
        public void AddProperty(string key,object value)
        {
            builder.SetProperty(key, value);
        }
        public string Create()
        {
            return builder.ToJson();
        }
    }
}
