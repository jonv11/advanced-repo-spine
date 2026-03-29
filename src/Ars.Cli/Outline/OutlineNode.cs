using System.Text.Json.Serialization;

namespace Ars.Cli.Outline;

public enum OutlineNodeType
{
    Directory,
    File,
    Heading
}

public sealed class OutlineNode
{
    public required OutlineNodeType Type { get; init; }
    public required string Name { get; init; }
    public string? Path { get; init; }
    public int? Level { get; init; }
    public bool Error { get; init; }
    public IReadOnlyList<OutlineNode> Children { get; init; } = [];
}
