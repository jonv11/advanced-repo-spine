using Ars.Cli.Model;

namespace Ars.Cli.Tests.Model;

public class ModelValidatorTests
{
    private static RepoModel ValidModel() => new()
    {
        Version = "1.0",
        Name = "Test",
        Rules = new ModelRules { CaseSensitive = false },
        Items = new List<ModelItem>
        {
            new() { Name = "readme", Type = "file", Path = "README.md", Required = true }
        }
    };

    [Fact]
    public void Validate_ValidModel_ReturnsNoErrors()
    {
        var errors = ModelValidator.Validate(ValidModel());
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_MissingVersion_ReturnsError()
    {
        var model = ValidModel();
        model.Version = "";
        var errors = ModelValidator.Validate(model);
        Assert.Contains(errors, e => e.Location == "version");
    }

    [Fact]
    public void Validate_UnsupportedVersion_ReturnsError()
    {
        var model = ValidModel();
        model.Version = "2.0";
        var errors = ModelValidator.Validate(model);
        Assert.Contains(errors, e => e.Location == "version" && e.Message.Contains("2.0"));
    }

    [Fact]
    public void Validate_MissingName_ReturnsError()
    {
        var model = ValidModel();
        model.Name = "";
        var errors = ModelValidator.Validate(model);
        Assert.Contains(errors, e => e.Location == "name");
    }

    [Fact]
    public void Validate_EmptyItems_ReturnsError()
    {
        var model = ValidModel();
        model.Items = new List<ModelItem>();
        var errors = ModelValidator.Validate(model);
        Assert.Contains(errors, e => e.Location == "items");
    }

    [Fact]
    public void Validate_ItemMissingName_ReturnsError()
    {
        var model = ValidModel();
        model.Items[0].Name = "";
        var errors = ModelValidator.Validate(model);
        Assert.Contains(errors, e => e.Message.Contains("name is required"));
    }

    [Fact]
    public void Validate_ItemInvalidType_ReturnsError()
    {
        var model = ValidModel();
        model.Items[0].Type = "symlink";
        var errors = ModelValidator.Validate(model);
        Assert.Contains(errors, e => e.Message.Contains("'directory' or 'file'"));
    }

    [Fact]
    public void Validate_ItemMissingPath_ReturnsError()
    {
        var model = ValidModel();
        model.Items[0].Path = "";
        var errors = ModelValidator.Validate(model);
        Assert.Contains(errors, e => e.Message.Contains("path is required"));
    }

    [Fact]
    public void Validate_BackslashInPath_ReturnsError()
    {
        var model = ValidModel();
        model.Items[0].Path = "docs\\README.md";
        var errors = ModelValidator.Validate(model);
        Assert.Contains(errors, e => e.Message.Contains("forward slashes"));
    }

    [Fact]
    public void Validate_DuplicatePaths_ReturnsError()
    {
        var model = ValidModel();
        model.Items.Add(new ModelItem { Name = "readme2", Type = "file", Path = "README.md" });
        var errors = ModelValidator.Validate(model);
        Assert.Contains(errors, e => e.Message.Contains("Duplicate path"));
    }

    [Fact]
    public void Validate_FileWithChildren_ReturnsError()
    {
        var model = ValidModel();
        model.Items[0].Children = new List<ModelItem>
        {
            new() { Name = "child", Type = "file", Path = "README.md/child.txt" }
        };
        var errors = ModelValidator.Validate(model);
        Assert.Contains(errors, e => e.Message.Contains("cannot have children"));
    }

    [Fact]
    public void Validate_ChildPathNotPrefixed_ReturnsError()
    {
        var model = new RepoModel
        {
            Version = "1.0",
            Name = "Test",
            Rules = new ModelRules(),
            Items = new List<ModelItem>
            {
                new()
                {
                    Name = "docs",
                    Type = "directory",
                    Path = "docs",
                    Children = new List<ModelItem>
                    {
                        new() { Name = "readme", Type = "file", Path = "other/README.md" }
                    }
                }
            }
        };

        var errors = ModelValidator.Validate(model);
        Assert.Contains(errors, e => e.Message.Contains("prefixed by parent path"));
    }

    [Fact]
    public void Validate_ValidHierarchy_ReturnsNoErrors()
    {
        var model = new RepoModel
        {
            Version = "1.0",
            Name = "Test",
            Rules = new ModelRules(),
            Items = new List<ModelItem>
            {
                new()
                {
                    Name = "docs",
                    Type = "directory",
                    Path = "docs",
                    Children = new List<ModelItem>
                    {
                        new() { Name = "readme", Type = "file", Path = "docs/README.md" }
                    }
                }
            }
        };

        var errors = ModelValidator.Validate(model);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_CaseInsensitive_DuplicatePaths_ReturnsError()
    {
        var model = new RepoModel
        {
            Version = "1.0",
            Name = "Test",
            Rules = new ModelRules { CaseSensitive = false },
            Items = new List<ModelItem>
            {
                new() { Name = "readme1", Type = "file", Path = "README.md", Required = true },
                new() { Name = "readme2", Type = "file", Path = "readme.md", Required = true }
            }
        };

        var errors = ModelValidator.Validate(model);
        Assert.Contains(errors, e => e.Code == "DUPLICATE_PATH" && e.Message.Contains("Duplicate path"));
    }

    [Fact]
    public void Validate_CaseSensitive_DifferentCasePaths_ReturnsNoError()
    {
        var model = new RepoModel
        {
            Version = "1.0",
            Name = "Test",
            Rules = new ModelRules { CaseSensitive = true },
            Items = new List<ModelItem>
            {
                new() { Name = "readme1", Type = "file", Path = "README.md", Required = true },
                new() { Name = "readme2", Type = "file", Path = "readme.md", Required = true }
            }
        };

        var errors = ModelValidator.Validate(model);
        Assert.DoesNotContain(errors, e => e.Code == "DUPLICATE_PATH");
    }

    [Fact]
    public void Validate_ErrorsHaveCorrectCodes()
    {
        var model = ValidModel();
        model.Version = "";
        var errors = ModelValidator.Validate(model);
        Assert.Contains(errors, e => e.Code == "MISSING_FIELD" && e.Location == "version");
    }

    [Fact]
    public void Validate_InvalidVersion_HasInvalidValueCode()
    {
        var model = ValidModel();
        model.Version = "2.0";
        var errors = ModelValidator.Validate(model);
        Assert.Contains(errors, e => e.Code == "INVALID_VALUE" && e.Location == "version");
    }

    [Fact]
    public void Validate_BackslashInPath_HasInvalidPathFormatCode()
    {
        var model = ValidModel();
        model.Items[0].Path = "docs\\README.md";
        var errors = ModelValidator.Validate(model);
        Assert.Contains(errors, e => e.Code == "INVALID_PATH_FORMAT");
    }
}
