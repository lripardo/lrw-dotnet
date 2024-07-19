namespace LRW.Core.Communication;

public abstract class Response(string id, int status, object? data)
{
    public string Id { get; } = id;
    public int Status { get; } = status;
    public object? Data { get; } = data;
}
