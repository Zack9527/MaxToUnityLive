using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Web.Script.Serialization;
namespace MaxServer_dll
{
    public class JsonHelper
    {
        public static string Serialize<T>(T t)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(t);
        }

        public static string SerializeFromObj(object obj)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(obj);
        }
        public static T Deserialize<T>(string json)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Deserialize<T>(json);
        }

        public static object DeserializeToObj(string json)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.DeserializeObject(json);
        }
        public static JsonBuilder CreateJsonObjectBuilder()
        {
            JsonBuilder builder = new JsonBuilder();
            return builder;
        }
        public static JsonBuilder CreateJsonArrayBuilder()
        {
            JsonBuilder builder = new JsonBuilder();
            return builder.ToArray();
        }
    }
    public class JsonBuilder
    {
        private JsonBuilder jsonBuilder;
        private Dictionary<string, object> JsonObject;
        private List<Dictionary<string, object>> JsonArray;

        public JsonBuilder()
        {
            jsonBuilder = this;
            JsonObject = new Dictionary<string, object>();
        }
        public JsonBuilder ToObject()
        {
            if (JsonArray != null && JsonArray.Count>0)
            {
                JsonObject = JsonArray[0];
            }
            else
            {
                JsonArray = null;
                JsonObject = new Dictionary<string, object>();
            }
            return jsonBuilder;
        }
        public JsonBuilder ToArray()
        {
            JsonArray = new List<Dictionary<string, object>>();
            if(JsonObject.Count>0)
                JsonArray.Add(JsonObject);
            return jsonBuilder;
        }
        public bool IsArray()
        {
            return JsonArray != null;
        }

        public object GetValue(string name)
        {
            if (IsArray())
                return null;
            if (JsonObject.ContainsKey(name))
                return JsonObject[name];
            return null;
        }

        public string GetStringValue(string name)
        {
            object value = GetValue(name);
            if (value == null)
                return string.Empty;
            else
                return value.ToString();
        }

        public JsonBuilder SetProperty(string name,object value)
        {
            if (IsArray())
                return jsonBuilder;
            if (typeof(JsonBuilder) == value.GetType())
            {
                JsonBuilder JsonValue = value as JsonBuilder;
                if (JsonValue.IsArray())
                    value = JsonValue.JsonArray;
                else
                    value = JsonValue.JsonObject;
            }
            if (JsonObject.ContainsKey(name))
            {
                JsonObject[name] = value;
            }
            else
            {
                JsonObject.Add(name, value);
            }
            
            return jsonBuilder;

        }
        public JsonBuilder RemoveProperty(string name)
        {
            if (IsArray())
                return jsonBuilder;
            if (JsonObject.ContainsKey(name))
            {
                JsonObject.Remove(name);
            }
            return jsonBuilder;
        }
        public JsonBuilder AddItem(JsonBuilder jb)
        {
            if (!IsArray())
                return jsonBuilder;
            if (jb.IsArray())
                JsonArray.AddRange(jb.JsonArray);
            else
                JsonArray.Add(jb.JsonObject);
            return jsonBuilder;
        }
        public JsonBuilder RemoveItem(JsonBuilder jb)
        {
            if (!IsArray())
                return jsonBuilder;
            if (jb.IsArray())
            {
                foreach (var deleItem in jb.JsonArray)
                {
                    if (JsonArray.Contains(deleItem))
                        JsonArray.Remove(deleItem);
                }
            }
            else
            {
                if (JsonArray.Contains(jb.JsonObject))
                    JsonArray.Remove(jb.JsonObject);
            }
            return jsonBuilder;
        }
        public string ToJson()
        {
            object ToSerialize;
            if (IsArray())
                ToSerialize = jsonBuilder.JsonArray;
            else
                ToSerialize = jsonBuilder.JsonObject;
            return JsonHelper.Serialize<dynamic>(ToSerialize);
        }
    }
}
