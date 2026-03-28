using System.Text.Json.Serialization;

namespace Ars.Cli.Comparison;

public sealed class ComparisonSummary
{
    [JsonPropertyName("missing")]
    public int Missing { get; set; }

    [JsonPropertyName("present")]
    public int Present { get; set; }

    [JsonPropertyName("optionalMissing")]
    public int OptionalMissing { get; set; }

    [JsonPropertyName("unmatched")]
    public int Unmatched { get; set; }

    [JsonPropertyName("misplaced")]
    public int Misplaced { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public sealed class ComparisonSettings
{
    [JsonPropertyName("caseSensitive")]
    public bool CaseSensitive { get; init; }

    [JsonPropertyName("ignoredPatterns")]
    public List<string> IgnoredPatterns { get; init; } = new();
}

public sealed class ComparisonResult
{
    [JsonPropertyName("modelVersion")]
    public required string ModelVersion { get; init; }

    [JsonPropertyName("modelName")]
    public required string ModelName { get; init; }

    [JsonPropertyName("root")]
    public required string Root { get; init; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; init; }

    [JsonPropertyName("settings")]
    public required ComparisonSettings Settings { get; init; }

    [JsonPropertyName("summary")]
    public required ComparisonSummary Summary { get; init; }

    [JsonPropertyName("findings")]
    public required List<Finding> Findings { get; init; }
}
