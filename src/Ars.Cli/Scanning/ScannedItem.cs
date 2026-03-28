namespace Ars.Cli.Scanning;

public sealed class ScannedItem
{
    public required string Path { get; init; }
    public required string Type { get; init; }
}
