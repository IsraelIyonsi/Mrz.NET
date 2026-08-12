using Mrz.Internal;
using Mrz.Tests.Fixtures;

namespace Mrz.Tests.Validation;

public sealed class Td3CheckDigitCorruptionTests
{
    public sealed record ExpectedInvalidFlags(bool DocumentNumber, bool DateOfBirth, bool DateOfExpiry, bool PersonalNumber, bool Composite);

    public static IEnumerable<object[]> CorruptionScenarios()
    {
        // (lineIndex, charIndex, replacement, expected-invalid flags)
        yield return new object[] { 1, Td3Layout.DocumentNumberOffset, 'M', new ExpectedInvalidFlags(true, false, false, false, true) };
        yield return new object[] { 1, Td3Layout.DocumentNumberCheckDigitOffset, '5', new ExpectedInvalidFlags(true, false, false, false, true) };
        yield return new object[] { 1, Td3Layout.DateOfBirthOffset, '8', new ExpectedInvalidFlags(false, true, false, false, true) };
        yield return new object[] { 1, Td3Layout.DateOfBirthCheckDigitOffset, '3', new ExpectedInvalidFlags(false, true, false, false, true) };
        yield return new object[] { 1, Td3Layout.DateOfExpiryOffset, '2', new ExpectedInvalidFlags(false, false, true, false, true) };
        yield return new object[] { 1, Td3Layout.DateOfExpiryCheckDigitOffset, '8', new ExpectedInvalidFlags(false, false, true, false, true) };
        yield return new object[] { 1, Td3Layout.PersonalNumberOffset, 'Y', new ExpectedInvalidFlags(false, false, false, true, true) };
        yield return new object[] { 1, Td3Layout.PersonalNumberCheckDigitOffset, '2', new ExpectedInvalidFlags(false, false, false, true, true) };
        yield return new object[] { 1, Td3Layout.CompositeCheckDigitOffset, '1', new ExpectedInvalidFlags(false, false, false, false, true) };
    }

    [Theory]
    [MemberData(nameof(CorruptionScenarios))]
    public void CorruptedField_FailsOnlyTheAffectedCheckDigits(int lineIndex, int charIndex, char replacement, ExpectedInvalidFlags expected)
    {
        string[] lines = Mutate(IcaoSpecimens.Td3Eriksson.Lines, lineIndex, charIndex, replacement);

        MrzDocument document = MrzParser.Parse(lines);

        Assert.Equal(!expected.DocumentNumber, document.Validation.DocumentNumberCheckDigitValid);
        Assert.Equal(!expected.DateOfBirth, document.Validation.DateOfBirthCheckDigitValid);
        Assert.Equal(!expected.DateOfExpiry, document.Validation.DateOfExpiryCheckDigitValid);
        Assert.Equal(!expected.PersonalNumber, document.Validation.PersonalNumberCheckDigitValid);
        Assert.Equal(!expected.Composite, document.Validation.CompositeCheckDigitValid);
        Assert.False(document.Validation.IsValid);
    }

    [Fact]
    public void CorruptedNationality_LeavesAllCheckDigitsValid()
    {
        string[] lines = Mutate(IcaoSpecimens.Td3Eriksson.Lines, 1, Td3Layout.NationalityOffset, 'C');

        MrzDocument document = MrzParser.Parse(lines);

        Assert.True(document.Validation.IsValid);
        Assert.NotEqual(IcaoSpecimens.Td3Eriksson.Nationality, document.Nationality);
    }

    [Fact]
    public void CorruptedSex_LeavesAllCheckDigitsValid()
    {
        string[] lines = Mutate(IcaoSpecimens.Td3Eriksson.Lines, 1, Td3Layout.SexOffset, 'M');

        MrzDocument document = MrzParser.Parse(lines);

        Assert.True(document.Validation.IsValid);
        Assert.Equal(MrzSex.Male, document.Sex);
    }

    private static string[] Mutate(string[] originalLines, int lineIndex, int charIndex, char replacement)
    {
        string[] lines = (string[])originalLines.Clone();
        char[] mutatedLine = lines[lineIndex].ToCharArray();
        mutatedLine[charIndex] = replacement;
        lines[lineIndex] = new string(mutatedLine);
        return lines;
    }
}
