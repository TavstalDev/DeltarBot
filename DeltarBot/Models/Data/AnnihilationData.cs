using System.Text.Json.Serialization;

namespace Tavstal.DeltarBot.Models.Data;

public class AnnihilationData
{
    [JsonPropertyName("last")]
    public DateTime Last { get; set; }
}