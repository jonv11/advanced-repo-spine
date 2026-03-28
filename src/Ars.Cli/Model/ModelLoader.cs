using System.Text.Json;

namespace Ars.Cli.Model;

public static class ModelLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static RepoModel Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Model file not found: {path}", path);

        string json = File.ReadAllText(path);
        return Parse(json);
    }

    public static RepoModel Parse(string json)
    {
        try
        {
            var model = JsonSerializer.Deserialize<RepoModel>(json, Options);
            return model ?? throw new InvalidOperationException("Model deserialized to null.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid JSON: {ex.Message}", ex);
        }
    }
}
