namespace Mrz.Tests.CheckDigit;

public sealed class MrzCheckDigitCalculatorTests
{
    public static IEnumerable<object[]> KnownIcaoFieldCheckDigits()
    {
        // ICAO Doc 9303 "ERIKSSON, ANNA MARIA" worked example, reused across Parts 3-6.
        yield return new object[] { "L898902C3", '6' };       // TD3 document number
        yield return new object[] { "D23145890", '7' };       // TD1/TD2/TD3 document number
        yield return new object[] { "740812", '2' };          // date of birth
        yield return new object[] { "120415", '9' };          // date of expiry
        yield return new object[] { "ZE184226B<<<<<", '1' };  // TD3 personal number
    }

    [Theory]
    [MemberData(nameof(KnownIcaoFieldCheckDigits))]
    public void Compute_MatchesKnownIcaoFieldCheckDigit(string field, char expectedCheckDigit)
    {
        Assert.Equal(expectedCheckDigit, MrzCheckDigitCalculator.Compute(field));
    }

    public static IEnumerable<object[]> KnownIcaoCompositeCheckDigits()
    {
        // TD3 composite input: doc#+check (10) + DOB+check (7) + expiry+check+personal#+check (22).
        yield return new object[] { "L898902C36" + "7408122" + "1204159ZE184226B<<<<<1", '0' };

        // TD2 composite input: doc#+check (10) + DOB+check (7) + expiry+check+optional data (14).
        yield return new object[] { "D2314589077408122" + "1204159<<<<<<<", '6' };

        // TD1 composite input: (doc#+check+optional1) (25) + DOB+check (7) + expiry+check (7) + optional2 (11).
        yield return new object[] { "D231458907<<<<<<<<<<<<<<<" + "7408122" + "1204159" + "<<<<<<<<<<<", '6' };
    }

    [Theory]
    [MemberData(nameof(KnownIcaoCompositeCheckDigits))]
    public void Compute_MatchesKnownIcaoCompositeCheckDigit(string compositeInput, char expectedCheckDigit)
    {
        Assert.Equal(expectedCheckDigit, MrzCheckDigitCalculator.Compute(compositeInput));
    }

    public static IEnumerable<object[]> WeightCyclingCases()
    {
        yield return new object[] { "1", '7' };          // position 0: weight 7, sum 7
        yield return new object[] { "01", '3' };         // position 1: weight 3, sum 3
        yield return new object[] { "001", '1' };        // position 2: weight 1, sum 1
        yield return new object[] { "0001", '7' };       // position 3: weight cycles back to 7
        yield return new object[] { "1111111", '9' };    // 7+3+1+7+3+1+7 = 29 -> 9
    }

    [Theory]
    [MemberData(nameof(WeightCyclingCases))]
    public void Compute_WeightsCycleAs731FromTheFirstCharacter(string field, char expectedCheckDigit)
    {
        Assert.Equal(expectedCheckDigit, MrzCheckDigitCalculator.Compute(field));
    }

    public static IEnumerable<object[]> CharacterValueCases()
    {
        // A single-character field has weight 7, so the check digit is (value * 7) mod 10.
        yield return new object[] { '0', 0 };
        yield return new object[] { '5', 5 };
        yield return new object[] { '9', 9 };
        yield return new object[] { 'A', 10 };
        yield return new object[] { 'M', 22 };
        yield return new object[] { 'Z', 35 };
        yield return new object[] { '<', 0 };
    }

    [Theory]
    [MemberData(nameof(CharacterValueCases))]
    public void Compute_SingleCharacterField_UsesIcaoCharacterValue(char character, int expectedValue)
    {
        char expectedCheckDigit = (char)('0' + (expectedValue * 7 % 10));
        Assert.Equal(expectedCheckDigit, MrzCheckDigitCalculator.Compute(character.ToString()));
    }

    [Fact]
    public void Compute_EmptyField_ReturnsZero()
    {
        Assert.Equal('0', MrzCheckDigitCalculator.Compute(string.Empty));
    }

    [Fact]
    public void Compute_AllFillerField_ReturnsZero()
    {
        Assert.Equal('0', MrzCheckDigitCalculator.Compute("<<<<<<<<<"));
    }

    [Theory]
    [InlineData(' ')]
    [InlineData('-')]
    [InlineData('#')]
    [InlineData('a')]
    [InlineData('_')]
    public void Compute_DisallowedCharacter_ThrowsMrzFormatException(char disallowedCharacter)
    {
        string field = "ABC" + disallowedCharacter + "123";
        Assert.Throws<MrzFormatException>(() => MrzCheckDigitCalculator.Compute(field));
    }

    [Fact]
    public void Verify_CorrectCheckDigit_ReturnsTrue()
    {
        Assert.True(MrzCheckDigitCalculator.Verify("L898902C3", '6'));
    }

    [Fact]
    public void Verify_IncorrectCheckDigit_ReturnsFalse()
    {
        Assert.False(MrzCheckDigitCalculator.Verify("L898902C3", '5'));
    }

    [Fact]
    public void Verify_FillerCheckDigitOnAllFillerField_ReturnsTrue()
    {
        Assert.True(MrzCheckDigitCalculator.Verify("<<<<<<<<<<<<<<", '<'));
    }

    [Fact]
    public void Verify_FillerCheckDigitOnNonFillerField_ReturnsFalse()
    {
        Assert.False(MrzCheckDigitCalculator.Verify("ZE184226B<<<<<", '<'));
    }

    [Fact]
    public void Verify_DisallowedCharacterInField_ThrowsMrzFormatException()
    {
        Assert.Throws<MrzFormatException>(() => MrzCheckDigitCalculator.Verify("AB-12", '3'));
    }
}
