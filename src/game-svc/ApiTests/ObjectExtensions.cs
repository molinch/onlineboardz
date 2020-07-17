using System.Text.Json;

namespace ApiTests
{
    public static class ObjectExtensions
    {
        public static T Clone<T>(this T source)
        {
            var json = JsonSerializer.Serialize(source);
            var clone = JsonSerializer.Deserialize<T>(json);
            return clone!;
        }
    }
}
