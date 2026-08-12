using Mrz.Internal;

namespace Mrz.Tests.Internal;

public sealed class MrzCharsetValidatorTests
{
    [Theory]
    [InlineData("UTO")]
    [InlineData("ERIKSSON<<ANNA")]
    [InlineData("<<<<<")]
    public void EnsureLettersOrFiller_ValidField_DoesNotThrow(string field)
    {
        MrzCharsetValidator.EnsureLettersOrFiller(field, "field");
    }

    [Theory]
    [InlineData("UT0")]
    [InlineData("UTO ")]
    [InlineData("uto")]
    public void EnsureLettersOrFiller_InvalidField_ThrowsMrzFormatException(string field)
    {
        Assert.Throws<MrzFormatException>(() => MrzCharsetValidator.EnsureLettersOrFiller(field, "field"));
    }

    [Theory]
    [InlineData("L898902C3")]
    [InlineData("D23145890")]
    [InlineData("<<<<<<<<<")]
    public void EnsureAlphanumericOrFiller_ValidField_DoesNotThrow(string field)
    {
        MrzCharsetValidator.EnsureAlphanumericOrFiller(field, "field");
    }

    [Theory]
    [InlineData("L898-02C3")]
    [InlineData("l898902c3")]
    public void EnsureAlphanumericOrFiller_InvalidField_ThrowsMrzFormatException(string field)
    {
        Assert.Throws<MrzFormatException>(() => MrzCharsetValidator.EnsureAlphanumericOrFiller(field, "field"));
    }

    [Theory]
    [InlineData("740812")]
    [InlineData("000000")]
    public void EnsureDigits_ValidField_DoesNotThrow(string field)
    {
        MrzCharsetValidator.EnsureDigits(field, "field");
    }

    [Theory]
    [InlineData("74081<")]
    [InlineData("74081A")]
    public void EnsureDigits_InvalidField_ThrowsMrzFormatException(string field)
    {
        Assert.Throws<MrzFormatException>(() => MrzCharsetValidator.EnsureDigits(field, "field"));
    }

    [Theory]
    [InlineData("740812")]
    [InlineData("000000")]
    [InlineData("<<<<<<")]
    [InlineData("74<<<<")]
    [InlineData("<40812")]
    public void EnsureDigitsOrFiller_ValidField_DoesNotThrow(string field)
    {
        MrzCharsetValidator.EnsureDigitsOrFiller(field, "field");
    }

    [Theory]
    [InlineData("74081A")]
    [InlineData("7408 2")]
    public void EnsureDigitsOrFiller_InvalidField_ThrowsMrzFormatException(string field)
    {
        Assert.Throws<MrzFormatException>(() => MrzCharsetValidator.EnsureDigitsOrFiller(field, "field"));
    }

    [Theory]
    [InlineData('0')]
    [InlineData('9')]
    [InlineData('<')]
    public void EnsureCheckDigitCharacter_ValidCharacter_DoesNotThrow(char checkDigit)
    {
        MrzCharsetValidator.EnsureCheckDigitCharacter(checkDigit, "field");
    }

    [Theory]
    [InlineData('A')]
    [InlineData(' ')]
    public void EnsureCheckDigitCharacter_InvalidCharacter_ThrowsMrzFormatException(char checkDigit)
    {
        Assert.Throws<MrzFormatException>(() => MrzCharsetValidator.EnsureCheckDigitCharacter(checkDigit, "field"));
    }

    [Theory]
    [InlineData('0')]
    [InlineData('9')]
    public void EnsureCompositeCheckDigitCharacter_ValidCharacter_DoesNotThrow(char checkDigit)
    {
        MrzCharsetValidator.EnsureCompositeCheckDigitCharacter(checkDigit, "field");
    }

    [Theory]
    [InlineData('<')]
    [InlineData('A')]
    [InlineData(' ')]
    public void EnsureCompositeCheckDigitCharacter_InvalidCharacter_ThrowsMrzFormatException(char checkDigit)
    {
        Assert.Throws<MrzFormatException>(() => MrzCharsetValidator.EnsureCompositeCheckDigitCharacter(checkDigit, "field"));
    }

    [Theory]
    [InlineData('M')]
    [InlineData('F')]
    [InlineData('<')]
    [InlineData('X')]
    public void EnsureSexCharacter_ValidCharacter_DoesNotThrow(char sexCharacter)
    {
        MrzCharsetValidator.EnsureSexCharacter(sexCharacter);
    }

    [Theory]
    [InlineData('m')]
    [InlineData('0')]
    [InlineData('Q')]
    public void EnsureSexCharacter_InvalidCharacter_ThrowsMrzFormatException(char sexCharacter)
    {
        Assert.Throws<MrzFormatException>(() => MrzCharsetValidator.EnsureSexCharacter(sexCharacter));
    }
}
