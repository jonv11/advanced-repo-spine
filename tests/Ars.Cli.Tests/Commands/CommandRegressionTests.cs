using Ars.Cli.Commands;
using Ars.Cli.Infrastructure;
using Spectre.Console.Cli;

namespace Ars.Cli.Tests.Commands;

/// <summary>
/// Regression tests for BUG-001 (markup crash in validation errors),
/// BUG-002 (help page crash from description brackets), and
/// QOL-005 (backslash display in suggest header).
/// </summary>
public class CommandRegressionTests : IDisposable
{
    private readonly string _tempDir;

    public CommandRegressionTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ars-tests-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private CommandApp CreateApp()
    {
        var app = new CommandApp();
        app.Configure(config =>
        {
            config.SetApplicationName("ars");
            config.PropagateExceptions();

            config.AddCommand<InitCommand>("init");
            config.AddCommand<ValidateCommand>("validate");
            config.AddCommand<CompareCommand>("compare");
            config.AddCommand<ReportCommand>("report");
            config.AddCommand<SuggestCommand>("suggest");
            config.AddCommand<ExportCommand>("export");
            config.AddCommand<OutlineCommand>("outline");
        });
        return app;
    }

    private string WriteModel(string json)
    {
        var path = Path.Combine(_tempDir, $"model-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        return path;
    }

    // ---------------------------------------------------------------
    // BUG-001: Validation error display must not crash with Spectre
    // markup parsing. Commands must return exit code 2 (InvalidInput).
    // ---------------------------------------------------------------

    [Fact]
    public void Validate_InvalidModel_MissingName_ReturnsExitCode2()
    {
        var model = WriteModel("""{"version":"1.0","rules":{},"items":[{"name":"r","type":"file","path":"README.md","required":true}]}""");
        var app = CreateApp();

        var exitCode = app.Run(new[] { "validate", "--model", model });

        Assert.Equal(ExitCodes.InvalidInput, exitCode);
    }

    [Fact]
    public void Validate_InvalidModel_MissingVersion_ReturnsExitCode2()
    {
        var model = WriteModel("""{"name":"Test","rules":{},"items":[{"name":"r","type":"file","path":"README.md","required":true}]}""");
        var app = CreateApp();

        var exitCode = app.Run(new[] { "validate", "--model", model });

        Assert.Equal(ExitCodes.InvalidInput, exitCode);
    }

    [Fact]
    public void Validate_InvalidModel_MissingItems_ReturnsExitCode2()
    {
        var model = WriteModel("""{"version":"1.0","name":"Test","rules":{}}""");
        var app = CreateApp();

        var exitCode = app.Run(new[] { "validate", "--model", model });

        Assert.Equal(ExitCodes.InvalidInput, exitCode);
    }

    [Fact]
    public void Validate_InvalidModel_EmptyObject_ReturnsExitCode2()
    {
        var model = WriteModel("{}");
        var app = CreateApp();

        var exitCode = app.Run(new[] { "validate", "--model", model });

        Assert.Equal(ExitCodes.InvalidInput, exitCode);
    }

    [Fact]
    public void Validate_InvalidModel_BackslashPath_ReturnsExitCode2()
    {
        var model = WriteModel("""{"version":"1.0","name":"Test","rules":{},"items":[{"name":"r","type":"file","path":"src\\file.cs","required":true}]}""");
        var app = CreateApp();

        var exitCode = app.Run(new[] { "validate", "--model", model });

        Assert.Equal(ExitCodes.InvalidInput, exitCode);
    }

    [Fact]
    public void Validate_InvalidModel_NestedErrors_ReturnsExitCode2()
    {
        // items[0] has errors — location string contains brackets like "items[0]"
        var model = WriteModel("""{"version":"1.0","name":"Test","rules":{},"items":[{"name":"","type":"bad","path":"","required":true}]}""");
        var app = CreateApp();

        var exitCode = app.Run(new[] { "validate", "--model", model });

        Assert.Equal(ExitCodes.InvalidInput, exitCode);
    }

    [Fact]
    public void Compare_InvalidModel_ReturnsExitCode2()
    {
        var model = WriteModel("""{"version":"1.0","items":[]}""");
        var app = CreateApp();

        var exitCode = app.Run(new[] { "compare", "--model", model, "--root", _tempDir });

        Assert.Equal(ExitCodes.InvalidInput, exitCode);
    }

    [Fact]
    public void Report_InvalidModel_ReturnsExitCode2()
    {
        var model = WriteModel("""{"version":"1.0","items":[]}""");
        var app = CreateApp();

        var exitCode = app.Run(new[] { "report", "--model", model, "--root", _tempDir });

        Assert.Equal(ExitCodes.InvalidInput, exitCode);
    }

    [Fact]
    public void Export_InvalidModel_ReturnsExitCode2()
    {
        var model = WriteModel("""{"version":"1.0","items":[]}""");
        var app = CreateApp();

        var exitCode = app.Run(new[] { "export", "--model", model, "--root", _tempDir });

        Assert.Equal(ExitCodes.InvalidInput, exitCode);
    }

    [Fact]
    public void Suggest_InvalidModel_ReturnsExitCode2()
    {
        var model = WriteModel("""{"version":"1.0","items":[]}""");
        var app = CreateApp();

        var exitCode = app.Run(new[] { "suggest", "src/file.cs", "--model", model });

        Assert.Equal(ExitCodes.InvalidInput, exitCode);
    }

    [Fact]
    public void Validate_InvalidModelJson_ReturnsExitCode2()
    {
        var model = WriteModel("""{"version":"1.0","name":"Test","rules":{},"items":[{"name":"d","type":"file","path":"a.txt","required":true,"children":[{"name":"c","type":"file","path":"a.txt/c.txt","required":true}]}]}""");
        var app = CreateApp();

        var exitCode = app.Run(new[] { "validate", "--model", model });

        Assert.Equal(ExitCodes.InvalidInput, exitCode);
    }

    // ---------------------------------------------------------------
    // BUG-002: Help pages must render without crashing.
    // All 7 commands must return exit code 0 for --help.
    // ---------------------------------------------------------------

    [Theory]
    [InlineData("init")]
    [InlineData("validate")]
    [InlineData("compare")]
    [InlineData("report")]
    [InlineData("suggest")]
    [InlineData("export")]
    [InlineData("outline")]
    public void Help_AllCommands_DoNotCrash(string command)
    {
        var app = new CommandApp();
        app.Configure(config =>
        {
            config.SetApplicationName("ars");
            // Do NOT propagate exceptions here — we want to verify Spectre
            // renders help without throwing. A crash would be exit -1.

            config.AddCommand<InitCommand>("init");
            config.AddCommand<ValidateCommand>("validate");
            config.AddCommand<CompareCommand>("compare");
            config.AddCommand<ReportCommand>("report");
            config.AddCommand<SuggestCommand>("suggest");
            config.AddCommand<ExportCommand>("export");
            config.AddCommand<OutlineCommand>("outline");
        });

        var exitCode = app.Run(new[] { command, "--help" });

        Assert.Equal(0, exitCode);
    }

    // ---------------------------------------------------------------
    // QOL-005: Suggest header display should normalize backslashes
    // to forward slashes. Tested via SuggestionEngine (which already
    // normalizes) and the full command pipeline.
    // ---------------------------------------------------------------

    [Fact]
    public void Suggest_BackslashPath_ReturnsExitCode0_WithValidModel()
    {
        var model = WriteModel("""
        {
            "version": "1.0",
            "name": "Test",
            "rules": { "caseSensitive": false },
            "items": [
                { "name": "src", "type": "directory", "path": "src", "required": true }
            ]
        }
        """);
        var app = CreateApp();

        // The backslash input should not crash and should normalize in the display header.
        var exitCode = app.Run(new[] { "suggest", @"src\Program.cs", "--model", model });

        Assert.Equal(ExitCodes.Success, exitCode);
    }
}
