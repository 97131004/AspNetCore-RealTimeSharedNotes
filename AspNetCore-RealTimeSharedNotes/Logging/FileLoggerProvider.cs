using System.Text;
using System.Threading.Channels;

namespace AspNetCore_RealTimeSharedNotes.Logging;

public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly Channel<string> _channel; //thread-safe producer/consumer queue
    private readonly string _logFilePath;
    private readonly Task _writerTask;
    private readonly CancellationTokenSource _cts = new();

    public FileLoggerProvider(IConfiguration configuration, IWebHostEnvironment env)
    {
        //with CreateUnbounded the .TryWrite never blocks the caller, every error is guaranteed to be queued and eventually written
        _channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            AllowSynchronousContinuations = false
        });

        _logFilePath = Path.Combine(env.ContentRootPath, "Logging", "errors.log");
        _writerTask = ProcessQueueAsync(_cts.Token);
    }

    public ILogger CreateLogger(string categoryName) => new FileLogger(categoryName, _channel.Writer);

    internal static string BuildEntry(string category, Exception exception)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");
        sb.AppendLine($"Category: {category}");
        sb.AppendLine($"Message: {exception.Message}");
        sb.AppendLine($"StackTrace: {exception.StackTrace}");

        var inner = exception.InnerException;
        var depth = 1;
        while (inner != null)
        {
            sb.AppendLine($"InnerException (depth {depth}):");
            sb.AppendLine($"  Message    : {inner.Message}");
            sb.AppendLine($"  StackTrace : {inner.StackTrace}");
            inner = inner.InnerException;
            depth++;
        }

        sb.AppendLine("-------");
        return sb.ToString();
    }

    private async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        await foreach (var entry in _channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            try
            {
                await File.AppendAllTextAsync(_logFilePath, entry, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                //suppressing logging failures to avoid cascading errors / deadlocks
            }
        }
    }

    public void Dispose()
    {
        _channel.Writer.Complete();

        //wait for the background reader to drain what's already buffered
        if (!_writerTask.Wait(TimeSpan.FromSeconds(5)))
            _cts.Cancel(); //only cancel as a last resort after the drain timeout

        _cts.Dispose();
    }
}
