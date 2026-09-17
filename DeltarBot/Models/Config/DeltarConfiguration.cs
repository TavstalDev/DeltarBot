using Tavstal.DeltarBot.Models.Logging;

namespace Tavstal.DeltarBot.Models.Config;

public class DeltarConfiguration
{
    public ELogLevel LogLevel { get; } = ELogLevel.INFO;
    
    public ulong? DevelopmentGuildId { get; } = 1544840995429163049;
}