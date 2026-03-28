using System.Text.Json.Serialization;

namespace Ars.Cli.Model;

public sealed class RepoModel
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("rules")]
    public ModelRules Rules { get; set; } = new();

    [JsonPropertyName("items")]
    public List<ModelItem> Items { get; set; } = new();
}
