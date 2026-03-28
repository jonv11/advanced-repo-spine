using Ars.Cli.Infrastructure;
using Ars.Cli.Model;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ars.Cli.Commands;

public sealed class ValidateSettings : CommandSettings
{
    [CommandOption("--model")]
    public string Model { get; set; } = "ars.json";
}

public sealed class ValidateCommand : Command<ValidateSettings>
{
    public override int Execute(CommandContext context, ValidateSettings settings)
    {
        RepoModel model;
        try
        {
            model = ModelLoader.Load(settings.Model);
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

        var errors = ModelValidator.Validate(model);

        if (errors.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]Model is valid.[/]");
            return ExitCodes.Success;
        }

        AnsiConsole.MarkupLine($"[red]Validation failed with {errors.Count} error(s):[/]");
        foreach (var error in errors)
        {
            AnsiConsole.MarkupLine($"  [red]•[/] [{Markup.Escape(error.Location)}] {Markup.Escape(error.Message)}");
        }

        return ExitCodes.InvalidInput;
    }
}
