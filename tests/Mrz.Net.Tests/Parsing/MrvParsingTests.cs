using Mrz.Internal;
using Mrz.Tests.Fixtures;

namespace Mrz.Tests.Parsing;

public sealed class MrvParsingTests
{
    public static IEnumerable<object[]> VisaSpecimens()
    {
        yield return new object[] { IcaoSpecimens.MrvAEriksson };
        yield return new object[] { IcaoSpecimens.MrvBEriksson };
    }

    [Theory]
    [MemberData(nameof(VisaSpecimens))]
    public void Parse_DetectsVisaType_NotPassportOrIdCard(MrzSpecimen specimen)
    {
        MrzDocument document = MrzParser.Parse(specimen.Lines);

        Assert.Equal(specimen.DocumentType, document.DocumentType);
        Assert.NotEqual(MrzDocumentType.Td3, document.DocumentType);
        Assert.NotEqual(MrzDocumentType.Td2, document.DocumentType);
    }

    [Fact]
    public void Parse_MrvA_DetectedAsMrvA()
    {
        MrzDocument document = MrzParser.Parse(IcaoSpecimens.MrvAEriksson.Lines);

        Assert.Equal(MrzDocumentType.MrvA, document.DocumentType);
    }

    [Fact]
    public void Parse_MrvB_DetectedAsMrvB()
    {
        MrzDocument document = MrzParser.Parse(IcaoSpecimens.MrvBEriksson.Lines);

        Assert.Equal(MrzDocumentType.MrvB, document.DocumentType);
    }

    [Theory]
    [MemberData(nameof(VisaSpecimens))]
    public void Parse_ReturnsExpectedFields(MrzSpecimen specimen)
    {
        MrzDocument document = MrzParser.Parse(specimen.Lines);

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
    [MemberData(nameof(VisaSpecimens))]
    public void Parse_PerFieldCheckDigitsPass(MrzSpecimen specimen)
    {
        MrzDocument document = MrzParser.Parse(specimen.Lines);

        Assert.True(document.Validation.DocumentNumberCheckDigitValid);
        Assert.True(document.Validation.DateOfBirthCheckDigitValid);
        Assert.True(document.Validation.DateOfExpiryCheckDigitValid);
    }

    [Theory]
    [MemberData(nameof(VisaSpecimens))]
    public void Parse_HasNoCompositeOrPersonalNumberCheck(MrzSpecimen specimen)
    {
        MrzDocument document = MrzParser.Parse(specimen.Lines);

        // A machine-readable visa has no composite check digit and no independently
        // check-digited personal number; these are reported as null (not applicable), never a
        // false failure.
        Assert.Null(document.Validation.CompositeCheckDigitValid);
        Assert.Null(document.Validation.PersonalNumberCheckDigitValid);
    }

    [Theory]
    [MemberData(nameof(VisaSpecimens))]
    public void Parse_ValidVisa_IsValidAndNotFalselyFailed(MrzSpecimen specimen)
    {
        MrzDocument document = MrzParser.Parse(specimen.Lines);

        // The bug being fixed: a valid visa must not report a composite-check failure.
        Assert.NotEqual(false, document.Validation.CompositeCheckDigitValid);
        Assert.True(document.Validation.IsValid);
    }

    [Theory]
    [MemberData(nameof(VisaSpecimens))]
    public void TryParse_ValidVisa_ReturnsTrueAndDocument(MrzSpecimen specimen)
    {
        bool succeeded = MrzParser.TryParse(specimen.Lines, out MrzDocument? document);

        Assert.True(succeeded);
        Assert.NotNull(document);
        Assert.True(document!.Validation.IsValid);
    }

    [Fact]
    public void MrvA_LineLengths_MatchTd3Geometry()
    {
        Assert.Equal(MrvALayout.LineCount, IcaoSpecimens.MrvAEriksson.Lines.Length);
        Assert.All(IcaoSpecimens.MrvAEriksson.Lines, line => Assert.Equal(MrvALayout.LineLength, line.Length));
        Assert.Equal(Td3Layout.LineLength, MrvALayout.LineLength);
    }

    [Fact]
    public void MrvB_LineLengths_MatchTd2Geometry()
    {
        Assert.Equal(MrvBLayout.LineCount, IcaoSpecimens.MrvBEriksson.Lines.Length);
        Assert.All(IcaoSpecimens.MrvBEriksson.Lines, line => Assert.Equal(MrvBLayout.LineLength, line.Length));
        Assert.Equal(Td2Layout.LineLength, MrvBLayout.LineLength);
    }

    [Fact]
    public void Passport_SharingMrvAGeometry_StillDetectsAsTd3AndValidatesComposite()
    {
        // Regression: a 2x44 document whose line 1 begins with 'P' stays a passport, composite
        // check digit and all, exactly as before MRV support was added.
        MrzDocument document = MrzParser.Parse(IcaoSpecimens.Td3Eriksson.Lines);

        Assert.Equal(MrzDocumentType.Td3, document.DocumentType);
        Assert.True(document.Validation.CompositeCheckDigitValid);
        Assert.True(document.Validation.PersonalNumberCheckDigitValid);
        Assert.True(document.Validation.IsValid);
    }

    [Fact]
    public void IdCard_SharingMrvBGeometry_StillDetectsAsTd2()
    {
        // Regression: a 2x36 document whose line 1 does not begin with 'V' stays a TD2 ID card
        // with its composite check digit.
        MrzDocument document = MrzParser.Parse(IcaoSpecimens.Td2Eriksson.Lines);

        Assert.Equal(MrzDocumentType.Td2, document.DocumentType);
        Assert.True(document.Validation.CompositeCheckDigitValid);
        Assert.True(document.Validation.IsValid);
    }
}
