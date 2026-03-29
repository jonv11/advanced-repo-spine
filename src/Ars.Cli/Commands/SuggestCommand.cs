using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ars.Cli.Infrastructure;
using Ars.Cli.Model;
using Ars.Cli.Suggestion;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ars.Cli.Commands;

public sealed class SuggestSettings : CommandSettings
{
    [CommandArgument(0, "<path>")]
    [Description("Path or path-like hint to suggest placement for")]
    public string Path { get; set; } = string.Empty;

    [CommandOption("--model")]
    [Description("Path to the JSON model file [default: ars.json]")]
    public string Model { get; set; } = "ars.json";

    [CommandOption("--format")]
    [Description("Output format: text or json [default: text]")]
    public string Format { get; set; } = "text";
}

public sealed class SuggestCommand : Command<SuggestSettings>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public override int Execute(CommandContext context, SuggestSettings settings)
    {
        if (!OptionValidation.TryValidateFormat(settings.Format, out var formatError))
        {
            ErrorConsole.Stderr.MarkupLine($"[red]Error:[/] {Markup.Escape(formatError!)}");
            return ExitCodes.InvalidInput;
        }

        RepoModel model;
        try
        {
            model = ModelLoader.Load(settings.Model);
        }
        catch (FileNotFoundException ex)
        {
            ErrorConsole.Stderr.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
            return ExitCodes.InvalidInput;
        }
        catch (InvalidOperationException ex)
        {
            ErrorConsole.Stderr.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
            return ExitCodes.InvalidInput;
        }

        var validationErrors = ModelValidator.Validate(model);
        if (validationErrors.Count > 0)
        {
            ErrorConsole.Stderr.MarkupLine($"[red]Model validation failed with {validationErrors.Count} error(s):[/]");
            foreach (var error in validationErrors)
                ErrorConsole.Stderr.MarkupLine($"  [red]•[/] [{Markup.Escape(error.Location)}] {Markup.Escape(error.Message)}");
            return ExitCodes.InvalidInput;
        }

        var result = SuggestionEngine.Suggest(model, settings.Path);

        if (settings.Format.Equals("json", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
            return ExitCodes.Success;
        }

        AnsiConsole.MarkupLine($"[bold]Suggest:[/] {Markup.Escape(settings.Path)}");
        AnsiConsole.WriteLine();

        if (result.Suggestions.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(result.Message)}[/]");
            return ExitCodes.Success;
        }

        AnsiConsole.MarkupLine(Markup.Escape(result.Message));
        AnsiConsole.WriteLine();

        foreach (var suggestion in result.Suggestions)
        {
            var color = suggestion.Confidence switch
            {
                "high" => "green",
                "medium" => "yellow",
                _ => "dim"
            };
            AnsiConsole.MarkupLine($"  [{color}]•[/] [bold]{Markup.Escape(suggestion.Path)}[/] ({suggestion.Confidence})");
            AnsiConsole.MarkupLine($"    {Markup.Escape(suggestion.Reason)}");
        }

        return ExitCodes.Success;
    }
}
