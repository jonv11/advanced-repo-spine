using System.Text.Json;
using Ars.Cli.Outline;

namespace Ars.Cli.Tests.Outline;

public sealed class OutlineJsonRendererTests
{
    [Fact]
    public void Render_ValidJson_ParsesSuccessfully()
    {
        var root = MakeDirectoryNode("docs", "docs",
            MakeFileNode("README.md", "docs/README.md",
                MakeHeading(1, "Title")));

        var json = OutlineJsonRenderer.RenderToString(root);
        var doc = JsonDocument.Parse(json);

        Assert.Equal("directory", doc.RootElement.GetProperty("type").GetString());
    }

    [Fact]
    public void Render_HeadingNode_HasNoPathOrChildren()
    {
        var root = MakeFileNode("doc.md", "doc.md",
            MakeHeading(2, "Section"));

        var json = OutlineJsonRenderer.RenderToString(root);
        var doc = JsonDocument.Parse(json);

        var heading = doc.RootElement.GetProperty("children")[0];
        Assert.Equal("heading", heading.GetProperty("type").GetString());
        Assert.Equal("Section", heading.GetProperty("name").GetString());
        Assert.Equal(2, heading.GetProperty("level").GetInt32());
        Assert.False(heading.TryGetProperty("path", out _));
        Assert.False(heading.TryGetProperty("children", out _));
    }

    [Fact]
    public void Render_FileNode_OmitsErrorWhenFalse()
    {
        var root = MakeFileNode("doc.md", "doc.md");

        var json = OutlineJsonRenderer.RenderToString(root);
        var doc = JsonDocument.Parse(json);

        Assert.False(doc.RootElement.TryGetProperty("error", out _));
    }

    [Fact]
    public void Render_FileNodeWithError_IncludesErrorField()
    {
        var root = new OutlineNode
        {
            Type = OutlineNodeType.File,
            Name = "bad.md",
            Path = "bad.md",
            Error = true,
            Children = []
        };

        var json = OutlineJsonRenderer.RenderToString(root);
        var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.GetProperty("error").GetBoolean());
    }

    [Fact]
    public void Render_NodeTypes_SerializeAsLowercaseStrings()
    {
        var root = MakeDirectoryNode("root", "root",
            MakeFileNode("f.md", "root/f.md",
                MakeHeading(1, "H")));

        var json = OutlineJsonRenderer.RenderToString(root);
        var doc = JsonDocument.Parse(json);

        Assert.Equal("directory", doc.RootElement.GetProperty("type").GetString());
        Assert.Equal("file", doc.RootElement.GetProperty("children")[0].GetProperty("type").GetString());
        Assert.Equal("heading",
            doc.RootElement.GetProperty("children")[0].GetProperty("children")[0].GetProperty("type").GetString());
    }

    [Fact]
    public void Render_Deterministic_TwoRendersSameOutput()
    {
        var root = MakeDirectoryNode("docs", "docs",
            MakeFileNode("a.md", "docs/a.md", MakeHeading(1, "A")),
            MakeFileNode("b.md", "docs/b.md", MakeHeading(2, "B")));

        var json1 = OutlineJsonRenderer.RenderToString(root);
        var json2 = OutlineJsonRenderer.RenderToString(root);

        Assert.Equal(json1, json2);
    }

    [Fact]
    public void Render_DirectoryNode_HasPathAndChildren()
    {
        var root = MakeDirectoryNode("src", "src");

        var json = OutlineJsonRenderer.RenderToString(root);
        var doc = JsonDocument.Parse(json);

        Assert.Equal("directory", doc.RootElement.GetProperty("type").GetString());
        Assert.Equal("src", doc.RootElement.GetProperty("name").GetString());
        Assert.Equal("src", doc.RootElement.GetProperty("path").GetString());
        Assert.Equal(JsonValueKind.Array, doc.RootElement.GetProperty("children").ValueKind);
    }

    private static OutlineNode MakeHeading(int level, string name) => new()
    {
        Type = OutlineNodeType.Heading,
        Name = name,
        Level = level
    };

    private static OutlineNode MakeFileNode(string name, string path, params OutlineNode[] children) => new()
    {
        Type = OutlineNodeType.File,
        Name = name,
        Path = path,
        Children = children
    };

    private static OutlineNode MakeDirectoryNode(string name, string path, params OutlineNode[] children) => new()
    {
        Type = OutlineNodeType.Directory,
        Name = name,
        Path = path,
        Children = children
    };
}
