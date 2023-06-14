using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProviderDiff
{
    public static class ExtensionMethods
    {
        public static string ToJson(this object obj)
        {
            System.Web.Script.Serialization.JavaScriptSerializer javascriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string json = javascriptSerializer.Serialize(obj);

            return json;
        }
    }
}