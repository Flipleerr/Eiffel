// can't use System.Text.Json because this isn't .NET Core
using System;
using Newtonsoft.Json;

namespace Eiffel.Helpers
{
    public static class JsonHelper
    {
        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
