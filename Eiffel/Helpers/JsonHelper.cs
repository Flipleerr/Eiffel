// can't use System.Text.Json because this isn't .NET Core
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Eiffel.Helpers
{
    public static class JsonHelper
    {
        public static void Deserialize(string json)
        {
            JsonConvert.DeserializeObject(json);
        }
    }
}
