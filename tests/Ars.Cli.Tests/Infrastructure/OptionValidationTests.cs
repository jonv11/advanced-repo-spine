using Ars.Cli.Infrastructure;

namespace Ars.Cli.Tests.Infrastructure;

public class OptionValidationTests
{
    [Theory]
    [InlineData("text")]
    [InlineData("json")]
    [InlineData("Text")]
    [InlineData("JSON")]
    [InlineData("Json")]
    [InlineData("TEXT")]
    public void TryValidateFormat_ValidValues_ReturnsTrue(string format)
    {
        var result = OptionValidation.TryValidateFormat(format, out var errorMessage);

        Assert.True(result);
        Assert.Null(errorMessage);
    }

    [Theory]
    [InlineData("xml")]
    [InlineData("yaml")]
    [InlineData("csv")]
    [InlineData("")]
    [InlineData("jsn")]
    [InlineData("tex")]
    public void TryValidateFormat_InvalidValues_ReturnsFalse(string format)
    {
        var result = OptionValidation.TryValidateFormat(format, out var errorMessage);

        Assert.False(result);
        Assert.NotNull(errorMessage);
        Assert.Contains("Valid formats: text, json", errorMessage);
    }

    [Fact]
    public void TryValidateFormat_InvalidValue_ErrorMessageIncludesValue()
    {
        OptionValidation.TryValidateFormat("xml", out var errorMessage);

        Assert.Contains("'xml'", errorMessage!);
    }
}
