using Mrz;

namespace Mrz.Tests.Fixtures;

/// <summary>
/// The expected fields for a specimen MRZ used across multiple test classes.
/// </summary>
public sealed record MrzSpecimen(
    string Name,
    MrzDocumentType DocumentType,
    string[] Lines,
    string DocumentCode,
    string IssuingState,
    string Surname,
    string GivenNames,
    string DocumentNumber,
    string Nationality,
    string DateOfBirth,
    MrzSex Sex,
    string DateOfExpiry,
    string? PersonalNumber,
    string? SupplementalOptionalData);

/// <summary>
/// The worked "ERIKSSON, ANNA MARIA" example that ICAO Doc 9303 uses across Part 3 (check
/// digits), Part 4 (TD3 passports), Part 5 (TD1 ID cards), and Part 6 (TD2 ID cards and visas),
/// plus supplementary fixtures exercising name truncation and populated optional data fields.
/// Every line is built from field values rather than typed-out filler runs, so line length is
/// correct by construction; <see cref="IcaoSpecimenTests"/> still asserts the lengths
/// explicitly. The supplementary fixtures below reuse the document number, date of birth, and
/// date of expiry segments of the worked example verbatim wherever the composite check digit
/// would otherwise need recomputation, so their check digits stay independently verifiable by
/// hand.
/// </summary>
public static class IcaoSpecimens
{
    private const char Filler = '<';

    private static string Pad(string value, int totalLength) => value + new string(Filler, totalLength - value.Length);

    /// <summary>ICAO Doc 9303 Part 4, the canonical TD3 (passport) worked example.</summary>
    public static readonly MrzSpecimen Td3Eriksson = BuildTd3Eriksson();

    /// <summary>ICAO Doc 9303 Part 6, the canonical TD2 worked example.</summary>
    public static readonly MrzSpecimen Td2Eriksson = BuildTd2Eriksson();

    /// <summary>ICAO Doc 9303 Part 5, the canonical TD1 (ID card) worked example.</summary>
    public static readonly MrzSpecimen Td1Eriksson = BuildTd1Eriksson();

    /// <summary>
    /// A TD1 specimen whose name field is filled exactly to capacity with no trailing filler,
    /// exercising the truncation signal. Lines 1 and 2 are otherwise identical to
    /// <see cref="Td1Eriksson"/>, so the check digits remain valid without new arithmetic.
    /// </summary>
    public static readonly MrzSpecimen Td1TruncatedName = BuildTd1TruncatedName();

    /// <summary>
    /// A TD1 specimen with only a primary identifier (surname) and no secondary identifier.
    /// Lines 1 and 2 are otherwise identical to <see cref="Td1Eriksson"/>.
    /// </summary>
    public static readonly MrzSpecimen Td1SingleIdentifier = BuildTd1SingleIdentifier();

    /// <summary>
    /// A TD1 specimen whose optional data fields on both lines are populated rather than
    /// filler, exercising <see cref="MrzDocument.SupplementalOptionalData"/> and
    /// <see cref="MrzDocument.PersonalNumber"/> together. Its composite check digit is
    /// recomputed for the populated fields (see the worked calculation in
    /// <see cref="BuildTd1PopulatedOptionalData"/>).
    /// </summary>
    public static readonly MrzSpecimen Td1PopulatedOptionalData = BuildTd1PopulatedOptionalData();

    public static IEnumerable<MrzSpecimen> All()
    {
        yield return Td3Eriksson;
        yield return Td2Eriksson;
        yield return Td1Eriksson;
        yield return Td1TruncatedName;
        yield return Td1SingleIdentifier;
        yield return Td1PopulatedOptionalData;
    }

    private static MrzSpecimen BuildTd3Eriksson()
    {
        string line1 = "P<" + "UTO" + Pad("ERIKSSON<<ANNA<MARIA", 39);
        string line2 = "L898902C3" + "6" + "UTO" + "740812" + "2" + "F" + "120415" + "9"
            + Pad("ZE184226B", 14) + "1" + "0";

        return new MrzSpecimen(
            "ICAO 9303 Part 4 TD3 worked example (ERIKSSON)",
            MrzDocumentType.Td3,
            new[] { line1, line2 },
            "P",
            "UTO",
            "ERIKSSON",
            "ANNA MARIA",
            "L898902C3",
            "UTO",
            "740812",
            MrzSex.Female,
            "120415",
            "ZE184226B",
            null);
    }

    private static MrzSpecimen BuildTd2Eriksson()
    {
        string line1 = "I<" + "UTO" + Pad("ERIKSSON<<ANNA<MARIA", 31);
        string line2 = "D231458907" + "UTO" + "740812" + "2" + "F" + "120415" + "9"
            + new string(Filler, 7) + "6";

        return new MrzSpecimen(
            "ICAO 9303 Part 6 TD2 worked example (ERIKSSON)",
            MrzDocumentType.Td2,
            new[] { line1, line2 },
            "I",
            "UTO",
            "ERIKSSON",
            "ANNA MARIA",
            "D23145890",
            "UTO",
            "740812",
            MrzSex.Female,
            "120415",
            null,
            null);
    }

    private static MrzSpecimen BuildTd1Eriksson()
    {
        string line1 = "I<" + "UTO" + "D231458907" + new string(Filler, 15);
        string line2 = "740812" + "2" + "F" + "120415" + "9" + "UTO" + new string(Filler, 11) + "6";
        string line3 = Pad("ERIKSSON<<ANNA<MARIA", 30);

        return new MrzSpecimen(
            "ICAO 9303 Part 5 TD1 worked example (ERIKSSON)",
            MrzDocumentType.Td1,
            new[] { line1, line2, line3 },
            "I",
            "UTO",
            "ERIKSSON",
            "ANNA MARIA",
            "D23145890",
            "UTO",
            "740812",
            MrzSex.Female,
            "120415",
            null,
            null);
    }

    private static MrzSpecimen BuildTd1TruncatedName()
    {
        string line1 = "I<" + "UTO" + "D231458907" + new string(Filler, 15);
        string line2 = "740812" + "2" + "F" + "120415" + "9" + "UTO" + new string(Filler, 11) + "6";
        string line3 = "PATTERSON<<ELIZABETH<CHARLOTTE";

        return new MrzSpecimen(
            "TD1 specimen with the name field filled exactly (no trailing filler)",
            MrzDocumentType.Td1,
            new[] { line1, line2, line3 },
            "I",
            "UTO",
            "PATTERSON",
            "ELIZABETH CHARLOTTE",
            "D23145890",
            "UTO",
            "740812",
            MrzSex.Female,
            "120415",
            null,
            null);
    }

    private static MrzSpecimen BuildTd1SingleIdentifier()
    {
        string line1 = "I<" + "UTO" + "D231458907" + new string(Filler, 15);
        string line2 = "740812" + "2" + "F" + "120415" + "9" + "UTO" + new string(Filler, 11) + "6";
        string line3 = Pad("OKONKWO", 30);

        return new MrzSpecimen(
            "TD1 specimen with a surname only, no given names",
            MrzDocumentType.Td1,
            new[] { line1, line2, line3 },
            "I",
            "UTO",
            "OKONKWO",
            string.Empty,
            "D23145890",
            "UTO",
            "740812",
            MrzSex.Female,
            "120415",
            null,
            null);
    }

    /// <summary>
    /// Composite check digit worked calculation for this fixture:
    /// Input = line1[5..30) "D231458907AB1234567&lt;&lt;&lt;&lt;&lt;&lt;"
    ///       + line2[0..7) "7408122" + line2[8..15) "1204159" + line2[19..30) "XY98765&lt;&lt;&lt;&lt;"
    /// which is 25 + 7 + 7 + 11 = 50 characters. Applying the 7-3-1 weighted sum (letters A=10 .. Z=35,
    /// filler = 0) over that 50-character input yields a weighted sum of 991; 991 mod 10 = 1, the
    /// composite check digit below.
    /// </summary>
    private static MrzSpecimen BuildTd1PopulatedOptionalData()
    {
        string line1 = "I<" + "ESP" + "D231458907" + Pad("AB1234567", 15);
        string line2 = "740812" + "2" + "F" + "120415" + "9" + "ESP" + Pad("XY98765", 11) + "1";
        string line3 = Pad("MARTINEZ<<CARLOS<ALBERTO", 30);

        return new MrzSpecimen(
            "TD1 specimen with populated optional data fields on both lines",
            MrzDocumentType.Td1,
            new[] { line1, line2, line3 },
            "I",
            "ESP",
            "MARTINEZ",
            "CARLOS ALBERTO",
            "D23145890",
            "ESP",
            "740812",
            MrzSex.Female,
            "120415",
            "XY98765",
            "AB1234567");
    }
}
