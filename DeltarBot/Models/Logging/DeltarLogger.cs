using Tavstal.DeltarBot.Utils.Logging;

namespace Tavstal.DeltarBot.Models.Logging;

public class DeltarLogger
{
    private readonly Lock _logLock = new();
    private readonly string? _moduleName;
    private readonly string? _customLogFilePath;
    
    public DeltarLogger(string? moduleName = null, string? customLogFilePath = null)
    {
        _moduleName = moduleName;
        _customLogFilePath = customLogFilePath;
    }
    
    public void Log(ELogLevel logLevel, string message, Exception? exception)
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        if ((Program.Config?.LogLevel ?? ELogLevel.DEBUG) > logLevel)
            return;
        
        string text = LoggerHelper.Format(logLevel, message, _moduleName, exception);
        lock (_logLock)
        {
            try
            {
                Console.ForegroundColor = LoggerHelper.GetLogLevelColor(logLevel);
                Console.WriteLine(text);
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Logger Console Error] {ex.Message}");
            }
        }
        
        LoggerHelper.EnqueueLog(text, _customLogFilePath);
    }
}