using Ars.Cli.Model;
using Ars.Cli.Suggestion;

namespace Ars.Cli.Tests.Suggestion;

public class SuggestionEngineTests
{
    private static RepoModel SimpleModel(params ModelItem[] items) => new()
    {
        Version = "1.0",
        Name = "Test",
        Rules = new ModelRules { CaseSensitive = false },
        Items = new List<ModelItem>(items)
    };

    [Fact]
    public void Suggest_ExactPathMatch_HighConfidence()
    {
        var model = SimpleModel(new ModelItem { Name = "readme", Type = "file", Path = "docs/README.md", Required = true });

        var result = SuggestionEngine.Suggest(model, "docs/README.md");

        Assert.NotEmpty(result.Suggestions);
        Assert.Contains(result.Suggestions, s => s.Confidence == "high" && s.Path == "docs/README.md");
    }

    [Fact]
    public void Suggest_ParentDirectoryMatch_ReturnsSuggestion()
    {
        var model = SimpleModel(new ModelItem
        {
            Name = "docs",
            Type = "directory",
            Path = "docs",
            Required = true,
            Children = new List<ModelItem>
            {
                new() { Name = "guide", Type = "file", Path = "docs/guide.md", Required = false }
            }
        });

        // Query for something under docs/ that isn't an exact match
        var result = SuggestionEngine.Suggest(model, "docs/other.md");

        Assert.NotEmpty(result.Suggestions);
        Assert.Contains(result.Suggestions, s => s.Path == "docs");
    }

    [Fact]
    public void Suggest_FilenameMatch_ReturnsSuggestion()
    {
        var model = SimpleModel(new ModelItem { Name = "readme", Type = "file", Path = "docs/README.md", Required = true });

        var result = SuggestionEngine.Suggest(model, "README.md");

        Assert.NotEmpty(result.Suggestions);
        Assert.Contains(result.Suggestions, s => s.Path == "docs/README.md");
    }

    [Fact]
    public void Suggest_NoMatch_ReturnsEmptySuggestions()
    {
        var model = SimpleModel(new ModelItem { Name = "readme", Type = "file", Path = "docs/README.md", Required = true });

        var result = SuggestionEngine.Suggest(model, "completely-unrelated-xyz");

        Assert.Empty(result.Suggestions);
        Assert.Contains("no suggestion", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Suggest_InputIsNormalized()
    {
        var model = SimpleModel(new ModelItem { Name = "readme", Type = "file", Path = "docs/README.md", Required = true });

        // Use backslash path
        var result = SuggestionEngine.Suggest(model, "docs\\README.md");

        Assert.NotEmpty(result.Suggestions);
        Assert.Contains(result.Suggestions, s => s.Path == "docs/README.md");
    }

    [Fact]
    public void Suggest_NameSimilarityMatch_ReturnsSuggestion()
    {
        var model = SimpleModel(new ModelItem { Name = "readme", Type = "file", Path = "docs/README.md", Required = true });

        // "readme" is contained in the item name
        var result = SuggestionEngine.Suggest(model, "readme");

        Assert.NotEmpty(result.Suggestions);
    }
}
