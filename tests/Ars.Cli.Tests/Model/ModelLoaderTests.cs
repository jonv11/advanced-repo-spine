using Ars.Cli.Model;

namespace Ars.Cli.Tests.Model;

public class ModelLoaderTests
{
    [Fact]
    public void Parse_ValidJson_DeserializesCorrectly()
    {
        var json = """
        {
          "version": "1.0",
          "name": "Test",
          "rules": { "caseSensitive": false, "ignore": [".git/"] },
          "items": [
            { "name": "readme", "type": "file", "path": "README.md", "required": true }
          ]
        }
        """;

        var model = ModelLoader.Parse(json);

        Assert.Equal("1.0", model.Version);
        Assert.Equal("Test", model.Name);
        Assert.False(model.Rules.CaseSensitive);
        Assert.Single(model.Rules.Ignore);
        Assert.Single(model.Items);
        Assert.Equal("readme", model.Items[0].Name);
        Assert.Equal("file", model.Items[0].Type);
        Assert.Equal("README.md", model.Items[0].Path);
        Assert.True(model.Items[0].Required);
    }

    [Fact]
    public void Parse_InvalidJson_ThrowsInvalidOperationException()
    {
        var json = "{ not valid json }}}";
        Assert.Throws<InvalidOperationException>(() => ModelLoader.Parse(json));
    }

    [Fact]
    public void Parse_NullResult_ThrowsInvalidOperationException()
    {
        var json = "null";
        Assert.Throws<InvalidOperationException>(() => ModelLoader.Parse(json));
    }

    [Fact]
    public void Load_FileNotFound_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() => ModelLoader.Load("nonexistent-file.json"));
    }

    [Fact]
    public void Parse_WithChildItems_DeserializesHierarchy()
    {
        var json = """
        {
          "version": "1.0",
          "name": "Test",
          "rules": { "caseSensitive": false },
          "items": [
            {
              "name": "docs",
              "type": "directory",
              "path": "docs",
              "required": true,
              "children": [
                { "name": "readme", "type": "file", "path": "docs/README.md", "required": true }
              ]
            }
          ]
        }
        """;

        var model = ModelLoader.Parse(json);

        Assert.Single(model.Items);
        Assert.NotNull(model.Items[0].Children);
        Assert.Single(model.Items[0].Children!);
        Assert.Equal("docs/README.md", model.Items[0].Children![0].Path);
    }
}
