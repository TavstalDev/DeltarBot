using System.Text.Json.Serialization;

namespace Tavstal.DeltarBot.Models.Data;

public class DeltarData
{
    [JsonPropertyName("annihilation")]
    public AnnihilationData Annihilation { get; set; } = new();
}