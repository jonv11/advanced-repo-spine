using Ars.Cli.Comparison;
using Ars.Cli.Infrastructure;
using Ars.Cli.Model;
using Ars.Cli.Reporting;
using Ars.Cli.Scanning;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ars.Cli.Commands;

public sealed class CompareSettings : CommandSettings
{
    [CommandOption("--model")]
    public string Model { get; set; } = "ars.json";

    [CommandOption("--root")]
    public string Root { get; set; } = ".";

    [CommandOption("--format")]
    public string Format { get; set; } = "text";
}

public sealed class CompareCommand : Command<CompareSettings>
{
    public override int Execute(CommandContext context, CompareSettings settings)
    {
        return ExecuteCompare(settings.Model, settings.Root, settings.Format);
    }

    public static int ExecuteCompare(string modelPath, string root, string format)
    {
        RepoModel model;
        try
        {
            model = ModelLoader.Load(modelPath);
        }
        catch (FileNotFoundException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
            return ExitCodes.InvalidInput;
        }
        catch (InvalidOperationException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
            return ExitCodes.InvalidInput;
        }

        var validationErrors = ModelValidator.Validate(model);
        if (validationErrors.Count > 0)
        {
            AnsiConsole.MarkupLine($"[red]Model validation failed with {validationErrors.Count} error(s):[/]");
            foreach (var error in validationErrors)
                AnsiConsole.MarkupLine($"  [red]•[/] [{Markup.Escape(error.Location)}] {Markup.Escape(error.Message)}");
            return ExitCodes.InvalidInput;
        }

        List<ScannedItem> scanned;
        try
        {
            scanned = FileSystemScanner.Scan(root, model.Rules);
        }
        catch (DirectoryNotFoundException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
            return ExitCodes.InvalidInput;
        }

        var result = ComparisonEngine.Compare(model, scanned, root);

        if (format.Equals("json", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine(JsonExporter.Export(result));
        }
        else
        {
            ConsoleReporter.Render(result);
        }

        bool hasErrors = result.Summary.Missing > 0 || result.Summary.Misplaced > 0;
        return hasErrors ? ExitCodes.StructuralIssues : ExitCodes.Success;
    }
}
