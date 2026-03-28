using System.Text.Json.Serialization;

namespace Ars.Cli.Suggestion;

public sealed class Suggestion
{
    [JsonPropertyName("path")]
    public required string Path { get; init; }

    [JsonPropertyName("reason")]
    public required string Reason { get; init; }

    [JsonPropertyName("confidence")]
    public required string Confidence { get; init; }
}

public sealed class SuggestionResult
{
    [JsonPropertyName("input")]
    public required string Input { get; init; }

    [JsonPropertyName("suggestions")]
    public required List<Suggestion> Suggestions { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }
}
