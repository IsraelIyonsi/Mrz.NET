using Mrz.Internal;

namespace Mrz.Tests.Parsing;

/// <summary>
/// ICAO 9303 permits the filler character in date-of-birth positions the issuing authority does
/// not know, unlike date of expiry. These tests build a TD3 line 2 from scratch (rather than
/// mutating the ICAO specimen) so every check digit, including the composite, is recomputed for
/// the filler-bearing date of birth and stays independently verifiable.
/// </summary>
public sealed class MrzDateOfBirthFillerParsingTests
{
    private const string DocumentNumberSegment = "L898902C3" + "6";
    private const string Nationality = "UTO";
    private const string Sex = "F";
    private const string ExpirySegment = "120415" + "9";
    private const string PersonalNumberSegment = "ZE184226B<<<<<" + "1";
    private const string NameField = "ERIKSSON<<ANNA<MARIA";
    private static readonly string Line1 = "P<" + "UTO" + NameField + new string('<', Td3Layout.LineLength - 2 - 3 - NameField.Length);

    public static IEnumerable<object[]> DateOfBirthWithFillerCases()
    {
        yield return new object[] { "<<<<<<" }; // entirely unknown
        yield return new object[] { "7408<<" }; // day unknown
        yield return new object[] { "<<0812" }; // year unknown
    }

    [Theory]
    [MemberData(nameof(DateOfBirthWithFillerCases))]
    public void Parse_DateOfBirthContainingFiller_ParsesAndValidates(string dateOfBirth)
    {
        char dateOfBirthCheckDigit = MrzCheckDigitCalculator.Compute(dateOfBirth);
        string dateOfBirthAndCheckDigit = dateOfBirth + dateOfBirthCheckDigit;

        string compositeInput = DocumentNumberSegment + dateOfBirthAndCheckDigit + ExpirySegment + PersonalNumberSegment;
        char compositeCheckDigit = MrzCheckDigitCalculator.Compute(compositeInput);

        string line2 = DocumentNumberSegment + Nationality + dateOfBirthAndCheckDigit + Sex + ExpirySegment
            + PersonalNumberSegment + compositeCheckDigit;

        Assert.Equal(Td3Layout.LineLength, line2.Length);

        MrzDocument document = MrzParser.Parse(new[] { Line1, line2 });

        Assert.Equal(dateOfBirth, document.DateOfBirth);
        Assert.True(document.Validation.DateOfBirthCheckDigitValid);
        Assert.True(document.Validation.IsValid);
    }

    [Fact]
    public void Parse_DateOfBirthContainingLetter_ThrowsMrzFormatException()
    {
        const string dateOfBirth = "74081A";
        string dateOfBirthAndCheckDigit = dateOfBirth + "0";
        string line2 = DocumentNumberSegment + Nationality + dateOfBirthAndCheckDigit + Sex + ExpirySegment
            + PersonalNumberSegment + "0";

        Assert.Throws<MrzFormatException>(() => MrzParser.Parse(new[] { Line1, line2 }));
    }
}
