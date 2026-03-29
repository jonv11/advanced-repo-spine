using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ars.Cli.Outline;

public static class OutlineJsonRenderer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static void Render(OutlineNode root)
    {
        var dto = ToDto(root);
        Console.WriteLine(JsonSerializer.Serialize(dto, JsonOptions));
    }

    public static string RenderToString(OutlineNode root)
    {
        var dto = ToDto(root);
        return JsonSerializer.Serialize(dto, JsonOptions);
    }

    private static object ToDto(OutlineNode node)
    {
        return node.Type switch
        {
            OutlineNodeType.Heading => new HeadingDto(node.Name, node.Level!.Value),
            OutlineNodeType.File => node.Error
                ? new FileErrorDto(node.Name, node.Path!, node.Children.Select(ToDto).ToArray())
                : new FileDto(node.Name, node.Path!, node.Children.Select(ToDto).ToArray()),
            OutlineNodeType.Directory => new DirectoryDto(
                node.Name, node.Path!, node.Children.Select(ToDto).ToArray()),
            _ => throw new InvalidOperationException($"Unknown node type: {node.Type}")
        };
    }

    private sealed record HeadingDto(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("level")] int Level)
    {
        public HeadingDto(string name, int level) : this("heading", name, level) { }
    }

    private sealed record FileDto(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("path")] string Path,
        [property: JsonPropertyName("children")] object[] Children)
    {
        public FileDto(string name, string path, object[] children)
            : this("file", name, path, children) { }
    }

    private sealed record FileErrorDto(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("path")] string Path,
        [property: JsonPropertyName("error")] bool Error,
        [property: JsonPropertyName("children")] object[] Children)
    {
        public FileErrorDto(string name, string path, object[] children)
            : this("file", name, path, true, children) { }
    }

    private sealed record DirectoryDto(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("path")] string Path,
        [property: JsonPropertyName("children")] object[] Children)
    {
        public DirectoryDto(string name, string path, object[] children)
            : this("directory", name, path, children) { }
    }
}
