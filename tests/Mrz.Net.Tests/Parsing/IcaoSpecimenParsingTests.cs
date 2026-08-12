using Mrz.Internal;
using Mrz.Tests.Fixtures;

namespace Mrz.Tests.Parsing;

public sealed class IcaoSpecimenParsingTests
{
    public static IEnumerable<object[]> Specimens() => IcaoSpecimens.All().Select(specimen => new object[] { specimen });

    [Theory]
    [MemberData(nameof(Specimens))]
    public void LineLengths_MatchDocumentType(MrzSpecimen specimen)
    {
        int expectedLength = specimen.DocumentType switch
        {
            MrzDocumentType.Td1 => Td1Layout.LineLength,
            MrzDocumentType.Td2 => Td2Layout.LineLength,
            MrzDocumentType.Td3 => Td3Layout.LineLength,
            _ => throw new InvalidOperationException("Unhandled document type in test fixture."),
        };

        int expectedLineCount = specimen.DocumentType switch
        {
            MrzDocumentType.Td1 => Td1Layout.LineCount,
            MrzDocumentType.Td2 => Td2Layout.LineCount,
            MrzDocumentType.Td3 => Td3Layout.LineCount,
            _ => throw new InvalidOperationException("Unhandled document type in test fixture."),
        };

        Assert.Equal(expectedLineCount, specimen.Lines.Length);
        Assert.All(specimen.Lines, line => Assert.Equal(expectedLength, line.Length));
    }

    [Theory]
    [MemberData(nameof(Specimens))]
    public void Parse_ReturnsExpectedFields(MrzSpecimen specimen)
    {
        MrzDocument document = MrzParser.Parse(specimen.Lines);

        Assert.Equal(specimen.DocumentType, document.DocumentType);
        Assert.Equal(specimen.DocumentCode, document.DocumentCode);
        Assert.Equal(specimen.IssuingState, document.IssuingState);
        Assert.Equal(specimen.Surname, document.Surname);
        Assert.Equal(specimen.GivenNames, document.GivenNames);
        Assert.Equal(specimen.DocumentNumber, document.DocumentNumber);
        Assert.Equal(specimen.Nationality, document.Nationality);
        Assert.Equal(specimen.DateOfBirth, document.DateOfBirth);
        Assert.Equal(specimen.Sex, document.Sex);
        Assert.Equal(specimen.DateOfExpiry, document.DateOfExpiry);
        Assert.Equal(specimen.PersonalNumber, document.PersonalNumber);
        Assert.Equal(specimen.SupplementalOptionalData, document.SupplementalOptionalData);
        Assert.Equal(specimen.Lines, document.Lines);
    }

    [Theory]
    [MemberData(nameof(Specimens))]
    public void Parse_AllCheckDigitsPass(MrzSpecimen specimen)
    {
        MrzDocument document = MrzParser.Parse(specimen.Lines);

        Assert.True(document.Validation.DocumentNumberCheckDigitValid);
        Assert.True(document.Validation.DateOfBirthCheckDigitValid);
        Assert.True(document.Validation.DateOfExpiryCheckDigitValid);
        Assert.True(document.Validation.CompositeCheckDigitValid);
        Assert.True(document.Validation.IsValid);

        if (specimen.DocumentType == MrzDocumentType.Td3)
        {
            Assert.True(document.Validation.PersonalNumberCheckDigitValid);
        }
        else
        {
            Assert.Null(document.Validation.PersonalNumberCheckDigitValid);
        }
    }

    [Theory]
    [MemberData(nameof(Specimens))]
    public void Parse_JoinedWithNewlines_ProducesSameResultAsLineArray(MrzSpecimen specimen)
    {
        string joined = string.Join("\n", specimen.Lines);

        MrzDocument fromText = MrzParser.Parse(joined);
        MrzDocument fromLines = MrzParser.Parse(specimen.Lines);

        // MrzDocument.Lines is deliberately typed IReadOnlyList<string>, and Parse(string) versus
        // Parse(IReadOnlyList<string>) hand back different concrete collection types (List<string>
        // versus the caller's own array); compare it by sequence rather than by record equality.
        Assert.Equal(fromLines.Lines, fromText.Lines);
        Assert.Equal(fromLines with { Lines = Array.Empty<string>() }, fromText with { Lines = Array.Empty<string>() });
    }

    [Theory]
    [MemberData(nameof(Specimens))]
    public void Parse_WithWindowsLineEndings_ProducesSameResult(MrzSpecimen specimen)
    {
        string joined = string.Join("\r\n", specimen.Lines);

        MrzDocument document = MrzParser.Parse(joined);

        Assert.True(document.Validation.IsValid);
    }

    [Theory]
    [MemberData(nameof(Specimens))]
    public void TryParse_ValidSpecimen_ReturnsTrueAndDocument(MrzSpecimen specimen)
    {
        bool succeeded = MrzParser.TryParse(specimen.Lines, out MrzDocument? document);

        Assert.True(succeeded);
        Assert.NotNull(document);
        Assert.True(document!.Validation.IsValid);
    }
}
