using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ApiTests
{
    public static class ObjectExtensions
    {
        public static T DeepClone<T>(this T obj)
        {
            // it's not an efficient deep clone, but for unit testing we don't really care yet
            var serialized = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}
