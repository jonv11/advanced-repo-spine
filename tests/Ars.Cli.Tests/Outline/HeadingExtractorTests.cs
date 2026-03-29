using Ars.Cli.Outline;

namespace Ars.Cli.Tests.Outline;

public sealed class HeadingExtractorTests : IDisposable
{
    private readonly string _tempDir;

    public HeadingExtractorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ars-heading-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string WriteFile(string name, string content)
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void Extract_AtxH1ThroughH6_ReturnsAllInDocumentOrder()
    {
        var path = WriteFile("headings.md",
            """
            # H1
            ## H2
            ### H3
            #### H4
            ##### H5
            ###### H6
            """);

        var result = HeadingExtractor.Extract(path, maxLevel: null);

        Assert.Equal(6, result.Count);
        for (var i = 0; i < 6; i++)
        {
            Assert.Equal(OutlineNodeType.Heading, result[i].Type);
            Assert.Equal(i + 1, result[i].Level);
            Assert.Equal($"H{i + 1}", result[i].Name);
        }
    }

    [Fact]
    public void Extract_HeadingsInsideCodeFence_AreSkipped()
    {
        var path = WriteFile("fenced.md",
            """
            # Real Heading
            ```
            # Not a heading
            ## Also not a heading
            ```
            ## Another Real Heading
            """);

        var result = HeadingExtractor.Extract(path, maxLevel: null);

        Assert.Equal(2, result.Count);
        Assert.Equal("Real Heading", result[0].Name);
        Assert.Equal("Another Real Heading", result[1].Name);
    }

    [Fact]
    public void Extract_HeadingsInsideTildeFence_AreSkipped()
    {
        var path = WriteFile("tilde.md",
            """
            # Before
            ~~~
            # Inside tilde fence
            ~~~
            # After
            """);

        var result = HeadingExtractor.Extract(path, maxLevel: null);

        Assert.Equal(2, result.Count);
        Assert.Equal("Before", result[0].Name);
        Assert.Equal("After", result[1].Name);
    }

    [Fact]
    public void Extract_TrailingHashesStripped()
    {
        var path = WriteFile("trailing.md",
            """
            ## Title ##
            ### Another ### 
            """);

        var result = HeadingExtractor.Extract(path, maxLevel: null);

        Assert.Equal(2, result.Count);
        Assert.Equal("Title", result[0].Name);
        Assert.Equal("Another", result[1].Name);
    }

    [Fact]
    public void Extract_EmptyFile_ReturnsEmptyList()
    {
        var path = WriteFile("empty.md", "");

        var result = HeadingExtractor.Extract(path, maxLevel: null);

        Assert.Empty(result);
    }

    [Fact]
    public void Extract_NoHeadings_ReturnsEmptyList()
    {
        var path = WriteFile("nohd.md",
            """
            This is just some text.
            No headings here at all.
            Just paragraphs.
            """);

        var result = HeadingExtractor.Extract(path, maxLevel: null);

        Assert.Empty(result);
    }

    [Fact]
    public void Extract_MaxLevel_FiltersDeepHeadings()
    {
        var path = WriteFile("depth.md",
            """
            # H1
            ## H2
            ### H3
            #### H4
            """);

        var result = HeadingExtractor.Extract(path, maxLevel: 2);

        Assert.Equal(2, result.Count);
        Assert.Equal("H1", result[0].Name);
        Assert.Equal("H2", result[1].Name);
    }

    [Fact]
    public void Extract_NonExistentFile_ReturnsEmptyList()
    {
        var path = Path.Combine(_tempDir, "nonexistent.md");

        var result = HeadingExtractor.Extract(path, maxLevel: null);

        Assert.Empty(result);
    }

    [Fact]
    public void Extract_OnlyCodeFenceHeadings_ReturnsEmptyList()
    {
        var path = WriteFile("allfenced.md",
            """
            ```
            # Heading inside fence
            ## Another inside fence
            ```
            """);

        var result = HeadingExtractor.Extract(path, maxLevel: null);

        Assert.Empty(result);
    }

    [Fact]
    public void Extract_HeadingNodes_HaveCorrectType()
    {
        var path = WriteFile("type.md", "# Title");

        var result = HeadingExtractor.Extract(path, maxLevel: null);

        var heading = Assert.Single(result);
        Assert.Equal(OutlineNodeType.Heading, heading.Type);
        Assert.Null(heading.Path);
        Assert.Equal(1, heading.Level);
    }
}
