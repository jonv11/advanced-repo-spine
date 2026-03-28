using System.Text.Json;
using System.Text.Json.Serialization;
using Ars.Cli.Comparison;

namespace Ars.Cli.Reporting;

public static class JsonExporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static string Export(ComparisonResult result)
    {
        return JsonSerializer.Serialize(result, Options);
    }
}
