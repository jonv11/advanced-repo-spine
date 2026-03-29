using System.ComponentModel;
using Ars.Cli.Infrastructure;
using Ars.Cli.Outline;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ars.Cli.Commands;

public sealed class OutlineSettings : CommandSettings
{
    [CommandOption("--path <PATH>")]
    [Description("Root path to traverse. Defaults to the current working directory.")]
    public string? Path { get; set; }

    [CommandOption("--max-depth <N>")]
    [Description("Maximum filesystem depth to traverse. Omit for unlimited depth.")]
    public int? MaxDepth { get; set; }

    [CommandOption("--headings-depth <N>")]
    [Description("Maximum heading level to include (1–6). Omit for all heading levels.")]
    public int? HeadingsDepth { get; set; }

    [CommandOption("--format <FORMAT>")]
    [Description("Output format: text or json")]
    [DefaultValue("text")]
    public string Format { get; set; } = "text";
}

public sealed class OutlineCommand : Command<OutlineSettings>
{
    public override int Execute(CommandContext context, OutlineSettings settings)
    {
        if (!OptionValidation.TryValidateFormat(settings.Format, out var formatError))
        {
            ErrorConsole.Stderr.MarkupLine($"[red]Error:[/] {Markup.Escape(formatError!)}");
            return ExitCodes.InvalidInput;
        }

        if (settings.MaxDepth.HasValue && settings.MaxDepth.Value < 0)
        {
            ErrorConsole.Stderr.MarkupLine("[red]Error:[/] --max-depth must be a non-negative integer.");
            return ExitCodes.InvalidInput;
        }

        if (settings.HeadingsDepth.HasValue && (settings.HeadingsDepth.Value < 1 || settings.HeadingsDepth.Value > 6))
        {
            ErrorConsole.Stderr.MarkupLine("[red]Error:[/] --headings-depth must be between 1 and 6.");
            return ExitCodes.InvalidInput;
        }

        var targetPath = settings.Path ?? Directory.GetCurrentDirectory();

        OutlineNode root;
        try
        {
            root = OutlineScanner.Scan(targetPath, settings.MaxDepth, settings.HeadingsDepth);
        }
        catch (DirectoryNotFoundException ex)
        {
            ErrorConsole.Stderr.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
            return ExitCodes.InvalidInput;
        }

        if (settings.Format.Equals("json", StringComparison.OrdinalIgnoreCase))
        {
            OutlineJsonRenderer.Render(root);
        }
        else
        {
            OutlineTextRenderer.Render(root);
        }

        return ExitCodes.Success;
    }
}
