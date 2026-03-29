using System.Text.Json.Serialization;

namespace Ars.Cli.Model;

public sealed class ValidationResult
{
    [JsonPropertyName("success")]
    public required bool Success { get; init; }

    [JsonPropertyName("errors")]
    public required List<ValidationErrorDto> Errors { get; init; }
}

public sealed class ValidationErrorDto
{
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }

    [JsonPropertyName("location")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Location { get; init; }

    public static ValidationErrorDto FromValidationError(ValidationError error)
    {
        return new ValidationErrorDto
        {
            Code = error.Code,
            Message = error.Message,
            Location = error.Location
        };
    }
}
