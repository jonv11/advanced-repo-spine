using System.Text.RegularExpressions;

namespace Ars.Cli.Outline;

public static class HeadingExtractor
{
    private static readonly Regex AtxHeadingRegex = new(
        @"^(#{1,6})\s+(.+?)(\s+#+\s*)?$",
        RegexOptions.Compiled);

    private static readonly Regex FenceOpenRegex = new(
        @"^(`{3,}|~{3,})",
        RegexOptions.Compiled);

    public static List<OutlineNode> Extract(string filePath, int? maxLevel)
    {
        var headings = new List<OutlineNode>();

        string[] lines;
        try
        {
            lines = File.ReadAllLines(filePath);
        }
        catch
        {
            return headings;
        }

        var inCodeFence = false;
        var fenceMarker = string.Empty;

        foreach (var line in lines)
        {
            var fenceMatch = FenceOpenRegex.Match(line);
            if (fenceMatch.Success)
            {
                var token = fenceMatch.Groups[1].Value;
                if (!inCodeFence)
                {
                    inCodeFence = true;
                    fenceMarker = token[0] == '`' ? "```" : "~~~";
                }
                else if (line.TrimEnd().StartsWith(fenceMarker))
                {
                    inCodeFence = false;
                    fenceMarker = string.Empty;
                }
                continue;
            }

            if (inCodeFence)
                continue;

            var headingMatch = AtxHeadingRegex.Match(line);
            if (!headingMatch.Success)
                continue;

            var level = headingMatch.Groups[1].Value.Length;
            var text = headingMatch.Groups[2].Value.Trim();

            if (string.IsNullOrEmpty(text))
                continue;

            if (maxLevel.HasValue && level > maxLevel.Value)
                continue;

            headings.Add(new OutlineNode
            {
                Type = OutlineNodeType.Heading,
                Name = text,
                Level = level
            });
        }

        return headings;
    }
}
