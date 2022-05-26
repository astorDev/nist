using System.Collections.Concurrent;

using Newtonsoft.Json;

namespace Nist.Logs;

public class JsonStateConsoleLogger : ILogger
{
    public IDisposable BeginScope<TState>(TState state) => new Disposable();
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
    {
        if (state is not IEnumerable<KeyValuePair<string, object?>> properties)
        {
            return;
        }

        var props = properties
            .Where(k => k.Key != "{OriginalFormat}" && k.Value != null)
            .ToDictionary(o => o.Key, o => o.Value);
            
        Console.WriteLine(JsonConvert.SerializeObject(props));
    }

    public class Disposable : IDisposable
    {
        public void Dispose() => GC.SuppressFinalize(this);
    }
    
    public class Provider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, JsonStateConsoleLogger> loggers = new();

        public ILogger CreateLogger(string categoryName) => this.loggers.GetOrAdd(categoryName, s => new());

        public void Dispose() => GC.SuppressFinalize(this);
    }
}

public static class LoggingBuilderExtensions 
{
    public static void AddJsonStateConsole(this ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.AddProvider(new JsonStateConsoleLogger.Provider());
    }
}