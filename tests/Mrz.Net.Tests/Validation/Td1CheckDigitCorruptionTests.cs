using Mrz.Internal;
using Mrz.Tests.Fixtures;

namespace Mrz.Tests.Validation;

public sealed class Td1CheckDigitCorruptionTests
{
    public sealed record ExpectedInvalidFlags(bool DocumentNumber, bool DateOfBirth, bool DateOfExpiry, bool Composite);

    public static IEnumerable<object[]> CorruptionScenarios()
    {
        yield return new object[] { 0, Td1Layout.DocumentNumberOffset, 'C', new ExpectedInvalidFlags(true, false, false, true) };
        yield return new object[] { 0, Td1Layout.DocumentNumberCheckDigitOffset, '6', new ExpectedInvalidFlags(true, false, false, true) };
        yield return new object[] { 1, Td1Layout.DateOfBirthOffset, '8', new ExpectedInvalidFlags(false, true, false, true) };
        yield return new object[] { 1, Td1Layout.DateOfBirthCheckDigitOffset, '3', new ExpectedInvalidFlags(false, true, false, true) };
        yield return new object[] { 1, Td1Layout.DateOfExpiryOffset, '2', new ExpectedInvalidFlags(false, false, true, true) };
        yield return new object[] { 1, Td1Layout.DateOfExpiryCheckDigitOffset, '8', new ExpectedInvalidFlags(false, false, true, true) };
        yield return new object[] { 1, Td1Layout.CompositeCheckDigitOffset, '5', new ExpectedInvalidFlags(false, false, false, true) };
    }

    [Theory]
    [MemberData(nameof(CorruptionScenarios))]
    public void CorruptedField_FailsOnlyTheAffectedCheckDigits(int lineIndex, int charIndex, char replacement, ExpectedInvalidFlags expected)
    {
        string[] lines = Mutate(IcaoSpecimens.Td1Eriksson.Lines, lineIndex, charIndex, replacement);

        MrzDocument document = MrzParser.Parse(lines);

        Assert.Equal(!expected.DocumentNumber, document.Validation.DocumentNumberCheckDigitValid);
        Assert.Equal(!expected.DateOfBirth, document.Validation.DateOfBirthCheckDigitValid);
        Assert.Equal(!expected.DateOfExpiry, document.Validation.DateOfExpiryCheckDigitValid);
        Assert.Null(document.Validation.PersonalNumberCheckDigitValid);
        Assert.Equal(!expected.Composite, document.Validation.CompositeCheckDigitValid);
        Assert.False(document.Validation.IsValid);
    }

    [Fact]
    public void CorruptedOptionalData1_InvalidatesOnlyComposite()
    {
        // Optional data 1 has no independent check digit; it only feeds the composite.
        string[] lines = Mutate(IcaoSpecimens.Td1PopulatedOptionalData.Lines, 0, Td1Layout.OptionalData1Offset, 'C');

        MrzDocument document = MrzParser.Parse(lines);

        Assert.True(document.Validation.DocumentNumberCheckDigitValid);
        Assert.True(document.Validation.DateOfBirthCheckDigitValid);
        Assert.True(document.Validation.DateOfExpiryCheckDigitValid);
        Assert.Null(document.Validation.PersonalNumberCheckDigitValid);
        Assert.False(document.Validation.CompositeCheckDigitValid);
        Assert.False(document.Validation.IsValid);
    }

    [Fact]
    public void CorruptedOptionalData2_InvalidatesOnlyComposite()
    {
        // Optional data 2 has no independent check digit; it only feeds the composite.
        string[] lines = Mutate(IcaoSpecimens.Td1PopulatedOptionalData.Lines, 1, Td1Layout.OptionalData2Offset, 'C');

        MrzDocument document = MrzParser.Parse(lines);

        Assert.True(document.Validation.DocumentNumberCheckDigitValid);
        Assert.True(document.Validation.DateOfBirthCheckDigitValid);
        Assert.True(document.Validation.DateOfExpiryCheckDigitValid);
        Assert.Null(document.Validation.PersonalNumberCheckDigitValid);
        Assert.False(document.Validation.CompositeCheckDigitValid);
        Assert.False(document.Validation.IsValid);
    }

    [Fact]
    public void CorruptedNationality_LeavesAllCheckDigitsValid()
    {
        string[] lines = Mutate(IcaoSpecimens.Td1Eriksson.Lines, 1, Td1Layout.NationalityOffset, 'C');

        MrzDocument document = MrzParser.Parse(lines);

        Assert.True(document.Validation.IsValid);
        Assert.NotEqual(IcaoSpecimens.Td1Eriksson.Nationality, document.Nationality);
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
