using System.Text.Json.Serialization;

namespace Ars.Cli.Comparison;

public enum FindingType
{
    Missing,
    Present,
    OptionalMissing,
    Unmatched,
    Misplaced
}

public enum FindingSeverity
{
    Error,
    Warning,
    Info
}

public sealed class Finding
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FindingType Type { get; init; }

    [JsonPropertyName("severity")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FindingSeverity Severity { get; init; }

    [JsonPropertyName("expectedPath")]
    public string? ExpectedPath { get; init; }

    [JsonPropertyName("actualPath")]
    public string? ActualPath { get; init; }

    [JsonPropertyName("itemName")]
    public string? ItemName { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }
}
