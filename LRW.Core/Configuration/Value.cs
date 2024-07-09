using LRW.Helpers;

namespace LRW.Configuration;

public class Value(string value)
{
    public string String { get; } = value;
    public int Int => Convert.ToInt32(String);
    public bool Bool => string.Compare(String, "true", true) == 0;
    public string[] Strings => JsonHelper.Deserialize<string[]>(String) ?? [];
}