using Tavstal.DeltarBot.Models.Logging;

namespace Tavstal.DeltarBot.Models.Config;

public class DeltarConfiguration
{
    public ELogLevel LogLevel { get; private set; } = ELogLevel.INFO;
}