using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ars.Cli.Infrastructure;
using Ars.Cli.Model;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ars.Cli.Commands;

public sealed class ValidateSettings : CommandSettings
{
    [CommandOption("--model")]
    [Description("Path to the JSON model file [default: ars.json]")]
    public string Model { get; set; } = "ars.json";

    [CommandOption("--format")]
    [Description("Output format: text or json [default: text]")]
    public string Format { get; set; } = "text";
}

public sealed class ValidateCommand : Command<ValidateSettings>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public override int Execute(CommandContext context, ValidateSettings settings)
    {
        bool isJson = settings.Format.Equals("json", StringComparison.OrdinalIgnoreCase);

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
            if (isJson)
            {
                var result = new Model.ValidationResult
                {
                    Success = false,
                    Errors = new List<ValidationErrorDto>
                    {
                        new() { Code = "FILE_NOT_FOUND", Message = ex.Message }
                    }
                };
                Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
            }
            else
            {
                ErrorConsole.Stderr.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
            }
            return ExitCodes.InvalidInput;
        }
        catch (InvalidOperationException ex)
        {
            if (isJson)
            {
                var result = new Model.ValidationResult
                {
                    Success = false,
                    Errors = new List<ValidationErrorDto>
                    {
                        new() { Code = "INVALID_JSON", Message = ex.Message }
                    }
                };
                Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
            }
            else
            {
                ErrorConsole.Stderr.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
            }
            return ExitCodes.InvalidInput;
        }

        var errors = ModelValidator.Validate(model);

        if (isJson)
        {
            var result = new Model.ValidationResult
            {
                Success = errors.Count == 0,
                Errors = errors.Select(ValidationErrorDto.FromValidationError).ToList()
            };
            Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
            return errors.Count == 0 ? ExitCodes.Success : ExitCodes.InvalidInput;
        }

        if (errors.Count == 0)
        {
            ErrorConsole.Stderr.MarkupLine("[green]Model is valid.[/]");
            return ExitCodes.Success;
        }

        ErrorConsole.Stderr.MarkupLine($"[red]Validation failed with {errors.Count} error(s):[/]");
        foreach (var error in errors)
        {
            ErrorConsole.Stderr.MarkupLine($"  [red]•[/] [{Markup.Escape(error.Location)}] {Markup.Escape(error.Message)}");
        }

        return ExitCodes.InvalidInput;
    }
}
