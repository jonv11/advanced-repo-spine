using System.Text.Json.Serialization;

namespace Ars.Cli.Model;

public sealed class ModelRules
{
    [JsonPropertyName("caseSensitive")]
    public bool CaseSensitive { get; set; }

    [JsonPropertyName("ignore")]
    public List<string> Ignore { get; set; } = new();
}
