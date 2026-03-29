using Ars.Cli.Outline;

namespace Ars.Cli.Tests.Outline;

public sealed class OutlineScannerTests : IDisposable
{
    private readonly string _tempDir;

    public OutlineScannerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ars-scanner-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void Scan_DirectoryWithMixedContent_ReturnsCorrectTree()
    {
        Directory.CreateDirectory(Path.Combine(_tempDir, "subdir"));
        File.WriteAllText(Path.Combine(_tempDir, "README.md"), "# Hello");
        File.WriteAllText(Path.Combine(_tempDir, "data.txt"), "plain text");
        File.WriteAllText(Path.Combine(_tempDir, "subdir", "guide.md"), "## Guide");

        var result = OutlineScanner.Scan(_tempDir, maxDepth: null, headingsDepth: null);

        Assert.Equal(OutlineNodeType.Directory, result.Type);
        Assert.Equal(3, result.Children.Count);

        // Directories first, then files alphabetically
        Assert.Equal(OutlineNodeType.Directory, result.Children[0].Type);
        Assert.Equal("subdir", result.Children[0].Name);

        Assert.Equal(OutlineNodeType.File, result.Children[1].Type);
        Assert.Equal("data.txt", result.Children[1].Name);
        Assert.Empty(result.Children[1].Children); // non-md has no headings

        Assert.Equal(OutlineNodeType.File, result.Children[2].Type);
        Assert.Equal("README.md", result.Children[2].Name);
        Assert.Single(result.Children[2].Children);
        Assert.Equal("Hello", result.Children[2].Children[0].Name);
    }

    [Fact]
    public void Scan_DirectoriesFirstAlphabetical_ThenFilesAlphabetical()
    {
        Directory.CreateDirectory(Path.Combine(_tempDir, "zebra"));
        Directory.CreateDirectory(Path.Combine(_tempDir, "alpha"));
        File.WriteAllText(Path.Combine(_tempDir, "z.txt"), "");
        File.WriteAllText(Path.Combine(_tempDir, "a.txt"), "");

        var result = OutlineScanner.Scan(_tempDir, maxDepth: null, headingsDepth: null);

        Assert.Equal("alpha", result.Children[0].Name);
        Assert.Equal("zebra", result.Children[1].Name);
        Assert.Equal("a.txt", result.Children[2].Name);
        Assert.Equal("z.txt", result.Children[3].Name);
    }

    [Fact]
    public void Scan_MarkdownFileHasHeadingChildren()
    {
        File.WriteAllText(Path.Combine(_tempDir, "doc.md"), "# Title\n## Section");

        var result = OutlineScanner.Scan(_tempDir, maxDepth: null, headingsDepth: null);

        var file = result.Children[0];
        Assert.Equal(OutlineNodeType.File, file.Type);
        Assert.Equal(2, file.Children.Count);
        Assert.Equal(OutlineNodeType.Heading, file.Children[0].Type);
        Assert.Equal(1, file.Children[0].Level);
        Assert.Equal("Title", file.Children[0].Name);
    }

    [Fact]
    public void Scan_NonMarkdownFile_HasNoChildren()
    {
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), "{}");

        var result = OutlineScanner.Scan(_tempDir, maxDepth: null, headingsDepth: null);

        var file = result.Children[0];
        Assert.Empty(file.Children);
    }

    [Fact]
    public void Scan_MaxDepth0_ReturnsRootOnly()
    {
        Directory.CreateDirectory(Path.Combine(_tempDir, "sub"));
        File.WriteAllText(Path.Combine(_tempDir, "file.md"), "# H1");

        var result = OutlineScanner.Scan(_tempDir, maxDepth: 0, headingsDepth: null);

        Assert.Equal(OutlineNodeType.Directory, result.Type);
        Assert.Empty(result.Children);
    }

    [Fact]
    public void Scan_MaxDepth1_ReturnsRootAndDirectChildren()
    {
        var sub = Path.Combine(_tempDir, "sub");
        Directory.CreateDirectory(sub);
        File.WriteAllText(Path.Combine(sub, "nested.md"), "# Nested");
        File.WriteAllText(Path.Combine(_tempDir, "root.md"), "# Root");

        var result = OutlineScanner.Scan(_tempDir, maxDepth: 1, headingsDepth: null);

        Assert.Equal(2, result.Children.Count);
        // subdir is present but has no children (depth limit)
        var subNode = result.Children[0];
        Assert.Equal(OutlineNodeType.Directory, subNode.Type);
        Assert.Empty(subNode.Children);
        // root.md has headings (headings are within the file node, not a depth level)
        var rootFile = result.Children[1];
        Assert.Equal("root.md", rootFile.Name);
        Assert.Single(rootFile.Children);
    }

    [Fact]
    public void Scan_SingleMarkdownFile_ReturnsFileNodeWithHeadings()
    {
        var filePath = Path.Combine(_tempDir, "single.md");
        File.WriteAllText(filePath, "# Title\n## Sub");

        var result = OutlineScanner.Scan(filePath, maxDepth: null, headingsDepth: null);

        Assert.Equal(OutlineNodeType.File, result.Type);
        Assert.Equal("single.md", result.Name);
        Assert.Equal(2, result.Children.Count);
        Assert.Equal("Title", result.Children[0].Name);
        Assert.Equal("Sub", result.Children[1].Name);
    }

    [Fact]
    public void Scan_NonExistentPath_Throws()
    {
        var bad = Path.Combine(_tempDir, "does-not-exist");

        Assert.Throws<DirectoryNotFoundException>(() =>
            OutlineScanner.Scan(bad, maxDepth: null, headingsDepth: null));
    }

    [Fact]
    public void Scan_HeadinglessMarkdown_HasEmptyChildren()
    {
        File.WriteAllText(Path.Combine(_tempDir, "empty.md"), "No headings here.");

        var result = OutlineScanner.Scan(_tempDir, maxDepth: null, headingsDepth: null);

        var file = result.Children[0];
        Assert.Equal(OutlineNodeType.File, file.Type);
        Assert.Empty(file.Children);
        Assert.False(file.Error);
    }

    [Fact]
    public void Scan_PathsUseForwardSlashes()
    {
        var sub = Path.Combine(_tempDir, "docs");
        Directory.CreateDirectory(sub);
        File.WriteAllText(Path.Combine(sub, "readme.md"), "# Hi");

        var result = OutlineScanner.Scan(_tempDir, maxDepth: null, headingsDepth: null);

        var dir = result.Children[0];
        Assert.DoesNotContain("\\", dir.Path);
        var file = dir.Children[0];
        Assert.DoesNotContain("\\", file.Path);
    }

    [Fact]
    public void Scan_HeadingsDepth_FiltersDeepHeadings()
    {
        File.WriteAllText(Path.Combine(_tempDir, "doc.md"),
            "# H1\n## H2\n### H3\n#### H4");

        var result = OutlineScanner.Scan(_tempDir, maxDepth: null, headingsDepth: 2);

        var file = result.Children[0];
        Assert.Equal(2, file.Children.Count);
        Assert.Equal("H1", file.Children[0].Name);
        Assert.Equal("H2", file.Children[1].Name);
    }
}
