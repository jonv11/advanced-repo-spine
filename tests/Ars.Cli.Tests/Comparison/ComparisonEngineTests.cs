using Ars.Cli.Comparison;
using Ars.Cli.Model;
using Ars.Cli.Scanning;

namespace Ars.Cli.Tests.Comparison;

public class ComparisonEngineTests
{
    private static RepoModel SimpleModel(params ModelItem[] items) => new()
    {
        Version = "1.0",
        Name = "Test",
        Rules = new ModelRules { CaseSensitive = false },
        Items = new List<ModelItem>(items)
    };

    private static List<ScannedItem> Scanned(params (string path, string type)[] items)
        => items.Select(i => new ScannedItem { Path = i.path, Type = i.type }).ToList();

    [Fact]
    public void Compare_PresentRequiredItem_FindingTypeIsPresent()
    {
        var model = SimpleModel(new ModelItem { Name = "readme", Type = "file", Path = "README.md", Required = true });
        var scanned = Scanned(("README.md", "file"));

        var result = ComparisonEngine.Compare(model, scanned, ".");

        Assert.Contains(result.Findings, f => f.Type == FindingType.Present && f.ExpectedPath == "README.md");
        Assert.Equal(1, result.Summary.Present);
    }

    [Fact]
    public void Compare_MissingRequiredItem_FindingTypeIsMissing()
    {
        var model = SimpleModel(new ModelItem { Name = "readme", Type = "file", Path = "README.md", Required = true });
        var scanned = Scanned(); // empty

        var result = ComparisonEngine.Compare(model, scanned, ".");

        Assert.Contains(result.Findings, f => f.Type == FindingType.Missing && f.Severity == FindingSeverity.Error);
        Assert.Equal(1, result.Summary.Missing);
    }

    [Fact]
    public void Compare_AbsentOptionalItem_FindingTypeIsOptionalMissing()
    {
        var model = SimpleModel(new ModelItem { Name = "docs", Type = "directory", Path = "docs", Required = false });
        var scanned = Scanned();

        var result = ComparisonEngine.Compare(model, scanned, ".");

        Assert.Contains(result.Findings, f => f.Type == FindingType.OptionalMissing && f.Severity == FindingSeverity.Info);
        Assert.Equal(1, result.Summary.OptionalMissing);
    }

    [Fact]
    public void Compare_UnmatchedScannedItem_FindingTypeIsUnmatched()
    {
        var model = SimpleModel(new ModelItem { Name = "readme", Type = "file", Path = "README.md", Required = true });
        var scanned = Scanned(("README.md", "file"), ("extra.txt", "file"));

        var result = ComparisonEngine.Compare(model, scanned, ".");

        Assert.Contains(result.Findings, f => f.Type == FindingType.Unmatched && f.ActualPath == "extra.txt");
        Assert.Equal(1, result.Summary.Unmatched);
    }

    [Fact]
    public void Compare_MisplacedItem_SingleFilenameMatch_FindingTypeIsMisplaced()
    {
        var model = SimpleModel(new ModelItem { Name = "guide", Type = "file", Path = "docs/guide.md", Required = true });
        var scanned = Scanned(("guide.md", "file")); // at root instead of docs/

        var result = ComparisonEngine.Compare(model, scanned, ".");

        Assert.Contains(result.Findings, f =>
            f.Type == FindingType.Misplaced &&
            f.ExpectedPath == "docs/guide.md" &&
            f.ActualPath == "guide.md");
        Assert.Equal(1, result.Summary.Misplaced);
        Assert.Equal(0, result.Summary.Missing); // claimed by misplacement
    }

    [Fact]
    public void Compare_AmbiguousFilenameMatch_ClassifiesAsUnmatched()
    {
        // Two missing items with same filename — ambiguous, so both unmatched scan entries stay unmatched
        var model = SimpleModel(
            new ModelItem { Name = "guide1", Type = "file", Path = "docs/guide.md", Required = true },
            new ModelItem { Name = "guide2", Type = "file", Path = "other/guide.md", Required = true }
        );
        var scanned = Scanned(("guide.md", "file"));

        var result = ComparisonEngine.Compare(model, scanned, ".");

        // guide.md should be unmatched (ambiguous: could be docs/guide.md or other/guide.md)
        Assert.Contains(result.Findings, f => f.Type == FindingType.Unmatched && f.ActualPath == "guide.md");
        // Both expected items should be missing
        Assert.Equal(2, result.Summary.Missing);
    }

    [Fact]
    public void Compare_CaseInsensitive_MatchesDifferentCase()
    {
        var model = SimpleModel(new ModelItem { Name = "readme", Type = "file", Path = "README.md", Required = true });
        model.Rules.CaseSensitive = false;
        var scanned = Scanned(("readme.md", "file"));

        var result = ComparisonEngine.Compare(model, scanned, ".");

        Assert.Contains(result.Findings, f => f.Type == FindingType.Present);
    }

    [Fact]
    public void Compare_CaseSensitive_DoesNotMatchDifferentCase()
    {
        var model = SimpleModel(new ModelItem { Name = "readme", Type = "file", Path = "README.md", Required = true });
        model.Rules.CaseSensitive = true;
        var scanned = Scanned(("readme.md", "file"));

        var result = ComparisonEngine.Compare(model, scanned, ".");

        Assert.DoesNotContain(result.Findings, f => f.Type == FindingType.Present);
        Assert.Equal(1, result.Summary.Missing);
    }

    [Fact]
    public void Compare_DeterministicOrdering_SameInputSameOutput()
    {
        var model = SimpleModel(
            new ModelItem { Name = "a", Type = "file", Path = "a.txt", Required = true },
            new ModelItem { Name = "b", Type = "file", Path = "b.txt", Required = true }
        );
        var scanned = Scanned(("c.txt", "file"), ("d.txt", "file"));

        var result1 = ComparisonEngine.Compare(model, scanned, ".");
        var result2 = ComparisonEngine.Compare(model, scanned, ".");

        Assert.Equal(result1.Findings.Count, result2.Findings.Count);
        for (int i = 0; i < result1.Findings.Count; i++)
        {
            Assert.Equal(result1.Findings[i].Type, result2.Findings[i].Type);
            Assert.Equal(result1.Findings[i].ExpectedPath, result2.Findings[i].ExpectedPath);
            Assert.Equal(result1.Findings[i].ActualPath, result2.Findings[i].ActualPath);
        }
    }

    [Fact]
    public void Compare_EmptyModelItems_AllScannedUnmatched()
    {
        // Edge case: model with items but they don't match anything
        var model = SimpleModel(new ModelItem { Name = "readme", Type = "file", Path = "README.md", Required = true });
        var scanned = Scanned(("other.txt", "file"), ("another.txt", "file"));

        var result = ComparisonEngine.Compare(model, scanned, ".");

        Assert.Equal(1, result.Summary.Missing);
        Assert.Equal(2, result.Summary.Unmatched);
    }

    [Fact]
    public void Compare_EmptyScan_AllRequiredMissing()
    {
        var model = SimpleModel(
            new ModelItem { Name = "a", Type = "file", Path = "a.txt", Required = true },
            new ModelItem { Name = "b", Type = "file", Path = "b.txt", Required = true }
        );
        var scanned = Scanned();

        var result = ComparisonEngine.Compare(model, scanned, ".");

        Assert.Equal(2, result.Summary.Missing);
        Assert.Equal(0, result.Summary.Unmatched);
    }

    [Fact]
    public void Compare_NestedModelItems_FlattenedCorrectly()
    {
        var model = SimpleModel(new ModelItem
        {
            Name = "docs",
            Type = "directory",
            Path = "docs",
            Required = true,
            Children = new List<ModelItem>
            {
                new() { Name = "readme", Type = "file", Path = "docs/README.md", Required = true }
            }
        });
        var scanned = Scanned(("docs", "directory"), ("docs/README.md", "file"));

        var result = ComparisonEngine.Compare(model, scanned, ".");

        Assert.Equal(2, result.Summary.Present);
        Assert.Equal(0, result.Summary.Missing);
    }

    [Fact]
    public void Compare_SummaryTotalMatchesFindingsCount()
    {
        var model = SimpleModel(
            new ModelItem { Name = "a", Type = "file", Path = "a.txt", Required = true },
            new ModelItem { Name = "b", Type = "file", Path = "b.txt", Required = false }
        );
        var scanned = Scanned(("a.txt", "file"), ("c.txt", "file"));

        var result = ComparisonEngine.Compare(model, scanned, ".");

        Assert.Equal(result.Findings.Count, result.Summary.Total);
    }
}
