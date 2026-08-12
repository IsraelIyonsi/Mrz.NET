using Mrz.Internal;
using Mrz.Tests.Fixtures;

namespace Mrz.Tests.Parsing;

public sealed class MrzParserFormatErrorTests
{
    [Fact]
    public void Parse_NullText_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => MrzParser.Parse((string)null!));
    }

    [Fact]
    public void Parse_NullLines_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => MrzParser.Parse((IReadOnlyList<string>)null!));
    }

    [Fact]
    public void TryParse_NullText_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => MrzParser.TryParse((string)null!, out _));
    }

    [Fact]
    public void TryParse_NullLines_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => MrzParser.TryParse((IReadOnlyList<string>)null!, out _));
    }

    [Fact]
    public void Parse_LineListContainingNull_ThrowsMrzFormatException()
    {
        string[] lines = { IcaoSpecimens.Td3Eriksson.Lines[0], null! };
        Assert.Throws<MrzFormatException>(() => MrzParser.Parse(lines));
    }

    public static IEnumerable<object[]> UnrecognizedLayouts()
    {
        yield return new object[] { Array.Empty<string>() };
        yield return new object[] { new[] { new string('A', 44) } };
        yield return new object[] { new[] { new string('A', 44), new string('A', 30) } };
        yield return new object[] { new[] { new string('A', 20), new string('A', 20) } };
        yield return new object[] { new[] { new string('A', 30), new string('A', 30), new string('A', 30), new string('A', 30) } };
        yield return new object[] { new[] { new string('A', 44), new string('A', 44), new string('A', 44) } };
    }

    [Theory]
    [MemberData(nameof(UnrecognizedLayouts))]
    public void Parse_UnrecognizedLineLayout_ThrowsMrzFormatException(string[] lines)
    {
        Assert.Throws<MrzFormatException>(() => MrzParser.Parse(lines));
    }

    [Theory]
    [MemberData(nameof(UnrecognizedLayouts))]
    public void TryParse_UnrecognizedLineLayout_ReturnsFalse(string[] lines)
    {
        bool succeeded = MrzParser.TryParse(lines, out MrzDocument? document);

        Assert.False(succeeded);
        Assert.Null(document);
    }

    public static IEnumerable<object[]> InvalidCharacterMutations()
    {
        // (lineIndex, charIndex, replacement) applied to the TD3 ERIKSSON specimen.
        yield return new object[] { 0, 5, 'e' };   // lowercase letter in the name field
        yield return new object[] { 0, 2, '0' };   // digit in the issuing-state field
        yield return new object[] { 1, 0, ' ' };   // space in the document number field
        yield return new object[] { 1, 13, 'X' };  // letter in the date-of-birth field
        yield return new object[] { 1, 20, 'Q' };  // invalid sex marker
        yield return new object[] { 1, 0, '#' };   // symbol in the document number field
    }

    [Theory]
    [MemberData(nameof(InvalidCharacterMutations))]
    public void Parse_InvalidCharacter_ThrowsMrzFormatException(int lineIndex, int charIndex, char replacement)
    {
        string[] lines = (string[])IcaoSpecimens.Td3Eriksson.Lines.Clone();
        char[] mutatedLine = lines[lineIndex].ToCharArray();
        mutatedLine[charIndex] = replacement;
        lines[lineIndex] = new string(mutatedLine);

        Assert.Throws<MrzFormatException>(() => MrzParser.Parse(lines));
    }

    [Theory]
    [MemberData(nameof(InvalidCharacterMutations))]
    public void TryParse_InvalidCharacter_ReturnsFalse(int lineIndex, int charIndex, char replacement)
    {
        string[] lines = (string[])IcaoSpecimens.Td3Eriksson.Lines.Clone();
        char[] mutatedLine = lines[lineIndex].ToCharArray();
        mutatedLine[charIndex] = replacement;
        lines[lineIndex] = new string(mutatedLine);

        bool succeeded = MrzParser.TryParse(lines, out MrzDocument? document);

        Assert.False(succeeded);
        Assert.Null(document);
    }

    [Fact]
    public void Parse_FillerInCompositeCheckDigitPosition_ThrowsMrzFormatException()
    {
        // Unlike the other check-digit positions, ICAO 9303 never permits filler in the
        // composite check-digit position; it must be treated as a structural error.
        string[] lines = (string[])IcaoSpecimens.Td3Eriksson.Lines.Clone();
        char[] mutatedLine = lines[1].ToCharArray();
        mutatedLine[Td3Layout.CompositeCheckDigitOffset] = '<';
        lines[1] = new string(mutatedLine);

        Assert.Throws<MrzFormatException>(() => MrzParser.Parse(lines));
    }

    [Fact]
    public void Parse_BlankLinesAroundText_AreIgnored()
    {
        string text = "\n\n" + string.Join("\n", IcaoSpecimens.Td3Eriksson.Lines) + "\n\n";

        MrzDocument document = MrzParser.Parse(text);

        Assert.True(document.Validation.IsValid);
    }
}
