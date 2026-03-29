namespace Ars.Cli.Infrastructure;

public static class OptionValidation
{
    private static readonly string[] ValidFormats = { "text", "json" };

    public static bool TryValidateFormat(string value, out string? errorMessage)
    {
        if (ValidFormats.Any(f => f.Equals(value, StringComparison.OrdinalIgnoreCase)))
        {
            errorMessage = null;
            return true;
        }

        errorMessage = $"Unknown format '{value}'. Valid formats: {string.Join(", ", ValidFormats)}";
        return false;
    }
}
