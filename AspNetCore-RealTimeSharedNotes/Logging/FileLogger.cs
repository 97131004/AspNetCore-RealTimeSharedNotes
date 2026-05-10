using System.Threading.Channels;

namespace AspNetCore_RealTimeSharedNotes.Logging;

public sealed class FileLogger : ILogger
{
    private readonly string _category;
    private readonly ChannelWriter<string> _writer;

    public FileLogger(string category, ChannelWriter<string> writer)
    {
        _category = category;
        _writer = writer;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Error;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel) || exception is null)
            return;

        var entry = FileLoggerProvider.BuildEntry(_category, exception);
        _writer.TryWrite(entry);
    }
}
