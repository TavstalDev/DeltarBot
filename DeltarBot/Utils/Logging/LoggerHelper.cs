using System.Collections.Concurrent;
using Tavstal.DeltarBot.Models.Logging;

namespace Tavstal.DeltarBot.Utils.Logging;

public static class LoggerHelper
{
    private static readonly ConcurrentDictionary<string, ConcurrentQueue<string>> _queues = new();
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _fileLocks = new();
    private static readonly SemaphoreSlim _signal = new(0);
    public static string DefaultLogFilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Enqueues a log entry for asynchronous file writing.
    /// </summary>
    /// <param name="entry">The raw log message (without timestamp) to enqueue.</param>
    /// <param name="logFilePath">Optional target file path. If <see langword="null"/>, <see cref="DefaultLogFilePath"/> is used.</param>
    public static void EnqueueLog(string entry, string? logFilePath = null)
    {
        if (string.IsNullOrEmpty(logFilePath) && string.IsNullOrEmpty(DefaultLogFilePath))
            return;
        
        string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {entry}";
        var queue = _queues.GetOrAdd(logFilePath ?? DefaultLogFilePath, _ => new ConcurrentQueue<string>());
        queue.Enqueue(logEntry);
        _signal.Release();
    }
    
    /// <summary>
    /// Continuously processes queued log entries and appends them to their target files.
    /// </summary>
    /// <param name="token">Cancellation token used to stop the processing loop.</param>
    /// <returns>A task representing the background writer loop.</returns>
    public static async Task ProcessLogQueueAsync(CancellationToken token = default)
    {
        while (!token.IsCancellationRequested)
        {
            await _signal.WaitAsync(token);

            foreach (var (path, queue) in _queues)
            {
                if (!queue.TryDequeue(out var line))
                    continue;

                var fileLock = _fileLocks.GetOrAdd(path, _ => new SemaphoreSlim(1, 1));
                await fileLock.WaitAsync(token);
                try
                {
                    await File.AppendAllTextAsync(path, line + Environment.NewLine, token);
                }
                finally
                {
                    fileLock.Release();
                }
            }
        }
    }
    
    public static string Format(ELogLevel level, string message, string? moduleName = null, Exception? exception = null)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        var sb = new System.Text.StringBuilder();
        sb.Append($"[{timestamp}] [{level}]");
        if (!string.IsNullOrEmpty(moduleName))
            sb.Append($" [{moduleName}]");
        sb.Append($" {message}");

        if (exception != null)
        {
            sb.AppendLine();
            sb.Append($"└── Exception: {exception.GetType().Name}: {exception.Message}");
            sb.AppendLine();
            sb.Append(exception.StackTrace);
        }

        return sb.ToString();
    }
    
    public static ConsoleColor GetLogLevelColor(ELogLevel logLevel)
    {
        return logLevel switch
        {
            ELogLevel.DEBUG => ConsoleColor.Magenta,
            ELogLevel.INFO => ConsoleColor.Cyan,
            ELogLevel.WARN => ConsoleColor.Yellow,
            ELogLevel.ERROR => ConsoleColor.Red,
            ELogLevel.FATAL => ConsoleColor.DarkRed,
            _ => ConsoleColor.White
        };
    }
}