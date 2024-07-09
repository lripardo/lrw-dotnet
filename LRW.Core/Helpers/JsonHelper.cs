using System.Text.Json;

namespace LRW.Helpers;

public static class JsonHelper
{
    private readonly static JsonSerializerOptions options = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, options);
    }

    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, options);
    }
}
