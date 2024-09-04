using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace LRW.IntegrationTests;

public class XUnitLogger(ITestOutputHelper helper) : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        helper.WriteLine($"[{logLevel}] - {formatter(state, exception)}");
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
}
