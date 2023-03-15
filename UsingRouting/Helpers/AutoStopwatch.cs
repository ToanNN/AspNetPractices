using System.Diagnostics;

namespace UsingRouting.Helpers;

public sealed class AutoStopwatch : IDisposable
{
    private readonly ILogger _logger;
    private readonly string _message;
    private bool _disposed;
    private Stopwatch? _stopwatch;

    public AutoStopwatch(ILogger logger, string message)
    {
        (_logger, _message, _stopwatch) = (logger, message, Stopwatch.StartNew());
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _stopwatch?.Stop();
        _logger.LogInformation("{Message}:{ElapsedMilliseconds}ms", _message, _stopwatch?.ElapsedMilliseconds);
        _stopwatch = null;
    }
}