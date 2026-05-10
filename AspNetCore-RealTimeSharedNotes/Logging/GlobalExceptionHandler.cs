using Microsoft.AspNetCore.Diagnostics;

namespace AspNetCore_RealTimeSharedNotes.Logging;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, exception.Message);

        // Return false to let the built-in UseExceptionHandler continue
        // handling the response (redirect to /Home/Error etc.)
        return ValueTask.FromResult(false);
    }
}
