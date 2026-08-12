using Mrz.Internal;

namespace Mrz.Tests.Internal;

public sealed class MrzCharacterValueTests
{
    public static IEnumerable<object[]> ValidCharacterCases()
    {
        yield return new object[] { '0', 0 };
        yield return new object[] { '1', 1 };
        yield return new object[] { '9', 9 };
        yield return new object[] { 'A', 10 };
        yield return new object[] { 'B', 11 };
        yield return new object[] { 'Z', 35 };
        yield return new object[] { '<', 0 };
    }

    [Theory]
    [MemberData(nameof(ValidCharacterCases))]
    public void GetValue_ValidCharacter_ReturnsIcaoValue(char character, int expectedValue)
    {
        Assert.Equal(expectedValue, MrzCharacterValue.GetValue(character));
    }

    [Theory]
    [InlineData(' ')]
    [InlineData('a')]
    [InlineData('-')]
    [InlineData('.')]
    [InlineData('\t')]
    public void GetValue_InvalidCharacter_ThrowsMrzFormatException(char character)
    {
        Assert.Throws<MrzFormatException>(() => MrzCharacterValue.GetValue(character));
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("<", true)]
    [InlineData("<<<<<", true)]
    [InlineData("A<<<<", false)]
    [InlineData("<<<<A", false)]
    [InlineData("0", false)]
    public void IsAllFiller_ReturnsExpectedResult(string field, bool expected)
    {
        Assert.Equal(expected, MrzCharacterValue.IsAllFiller(field));
    }
}
