using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Tavstal.DeltarBot.Models.Logging;

[JsonConverter(typeof(StringEnumConverter))]
public enum ELogLevel
{
    DEBUG = 0,
    INFO = 1,
    WARN = 2,
    ERROR = 3,
    FATAL = 4
}