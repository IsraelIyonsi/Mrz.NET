using Mrz.Internal;
using Mrz.Tests.Fixtures;

namespace Mrz.Tests.Validation;

public sealed class MrvCheckDigitCorruptionTests
{
    public sealed record ExpectedInvalidFlags(bool DocumentNumber, bool DateOfBirth, bool DateOfExpiry);

    public static IEnumerable<object[]> MrvACorruptionScenarios()
    {
        yield return new object[] { MrvALayout.DocumentNumberOffset, 'M', new ExpectedInvalidFlags(true, false, false) };
        yield return new object[] { MrvALayout.DocumentNumberCheckDigitOffset, '5', new ExpectedInvalidFlags(true, false, false) };
        yield return new object[] { MrvALayout.DateOfBirthOffset, '8', new ExpectedInvalidFlags(false, true, false) };
        yield return new object[] { MrvALayout.DateOfBirthCheckDigitOffset, '3', new ExpectedInvalidFlags(false, true, false) };
        yield return new object[] { MrvALayout.DateOfExpiryOffset, '2', new ExpectedInvalidFlags(false, false, true) };
        yield return new object[] { MrvALayout.DateOfExpiryCheckDigitOffset, '8', new ExpectedInvalidFlags(false, false, true) };
    }

    [Theory]
    [MemberData(nameof(MrvACorruptionScenarios))]
    public void MrvA_CorruptedField_FailsOnlyTheAffectedCheckDigit_AndNeverProducesCompositeFailure(
        int charIndex, char replacement, ExpectedInvalidFlags expected)
    {
        string[] lines = Mutate(IcaoSpecimens.MrvAEriksson.Lines, 1, charIndex, replacement);

        MrzDocument document = MrzParser.Parse(lines);

        Assert.Equal(!expected.DocumentNumber, document.Validation.DocumentNumberCheckDigitValid);
        Assert.Equal(!expected.DateOfBirth, document.Validation.DateOfBirthCheckDigitValid);
        Assert.Equal(!expected.DateOfExpiry, document.Validation.DateOfExpiryCheckDigitValid);

        // Even a corrupt visa never gains a composite or personal-number check digit.
        Assert.Null(document.Validation.CompositeCheckDigitValid);
        Assert.Null(document.Validation.PersonalNumberCheckDigitValid);
        Assert.False(document.Validation.IsValid);
    }

    [Fact]
    public void MrvB_CorruptedDateOfBirthDigit_IsStillCaught()
    {
        string[] lines = Mutate(IcaoSpecimens.MrvBEriksson.Lines, 1, MrvBLayout.DateOfBirthOffset, '8');

        MrzDocument document = MrzParser.Parse(lines);

        Assert.False(document.Validation.DateOfBirthCheckDigitValid);
        Assert.True(document.Validation.DocumentNumberCheckDigitValid);
        Assert.True(document.Validation.DateOfExpiryCheckDigitValid);
        Assert.Null(document.Validation.CompositeCheckDigitValid);
        Assert.False(document.Validation.IsValid);
    }

    [Fact]
    public void MrvA_CorruptedOptionalData_LeavesAllCheckDigitsValid()
    {
        // The optional-data region carries no check digit, so mutating it must not fail the
        // document: there is no composite check digit to protect it.
        string[] lines = Mutate(IcaoSpecimens.MrvAEriksson.Lines, 1, MrvALayout.OptionalDataOffset, 'Q');

        MrzDocument document = MrzParser.Parse(lines);

        Assert.True(document.Validation.IsValid);
        Assert.Null(document.Validation.CompositeCheckDigitValid);
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
