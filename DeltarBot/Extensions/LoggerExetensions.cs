using Tavstal.DeltarBot.Models.Logging;

namespace Tavstal.DeltarBot.Extensions;

public static class LoggerExetensions
{
    public static void DEBUG(this DeltarLogger logger, string text, Exception? exception = null) =>
        logger.Log(ELogLevel.DEBUG, text, exception);
    
    public static void INFO(this DeltarLogger logger, string text, Exception? exception = null) =>
        logger.Log(ELogLevel.INFO, text, exception);
    
    public static void WARN(this DeltarLogger logger, string text, Exception? exception = null) =>
        logger.Log(ELogLevel.WARN, text, exception);
    
    public static void ERROR(this DeltarLogger logger, string text, Exception? exception = null) =>
        logger.Log(ELogLevel.ERROR, text, exception);
    
    public static void FATAL(this DeltarLogger logger, string text, Exception? exception = null) =>
        logger.Log(ELogLevel.FATAL, text, exception);
}