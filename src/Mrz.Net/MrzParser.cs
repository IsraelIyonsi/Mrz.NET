using Mrz.Internal;

namespace Mrz;

/// <summary>
/// Parses ICAO 9303 machine-readable-zone text into a typed <see cref="MrzDocument"/>, auto
/// detecting the TD1, TD2, or TD3 layout, or the MRV-A and MRV-B machine-readable visas, from
/// the number and length of lines supplied and the document code. A visa shares its geometry
/// with TD3 (MRV-A) or TD2 (MRV-B) and is told apart by a document code beginning with "V".
/// </summary>
/// <remarks>
/// Parsing only throws <see cref="MrzFormatException"/> for structural problems: an
/// unrecognized number or length of lines, or a character outside the set the position
/// permits. A wrong check digit is not a structural problem; it is reported on the returned
/// <see cref="MrzDocument"/> through <see cref="MrzDocument.Validation"/> so callers can inspect
/// exactly which field failed instead of losing the rest of the parse.
/// </remarks>
public static class MrzParser
{
    /// <summary>
    /// Parses machine-readable-zone text, splitting it into lines on line breaks.
    /// </summary>
    /// <param name="mrzText">The MRZ text, with lines separated by '\n', "\r\n", or '\r'.</param>
    /// <returns>The parsed document, including check-digit validation results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mrzText"/> is <see langword="null"/>.</exception>
    /// <exception cref="MrzFormatException">
    /// The text does not split into a recognized TD1, TD2, or TD3 line layout, or a field
    /// contains a character its position does not permit.
    /// </exception>
    public static MrzDocument Parse(string mrzText)
    {
        ArgumentNullException.ThrowIfNull(mrzText);
        return Parse(SplitLines(mrzText));
    }

    /// <summary>
    /// Parses machine-readable-zone text supplied as individual lines.
    /// </summary>
    /// <param name="lines">The MRZ lines, in document order.</param>
    /// <returns>The parsed document, including check-digit validation results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="lines"/> is <see langword="null"/>.</exception>
    /// <exception cref="MrzFormatException">
    /// The lines do not form a recognized TD1, TD2, or TD3 layout, or a field contains a
    /// character its position does not permit.
    /// </exception>
    public static MrzDocument Parse(IReadOnlyList<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        EnsureNoNullLines(lines);

        MrzDocumentType documentType = DetectDocumentType(lines);

        return documentType switch
        {
            MrzDocumentType.Td1 => ParseTd1(lines),
            MrzDocumentType.Td2 => ParseTd2(lines),
            MrzDocumentType.Td3 => ParseTd3(lines),
            MrzDocumentType.MrvA => ParseMrvA(lines),
            MrzDocumentType.MrvB => ParseMrvB(lines),
            _ => throw new MrzFormatException($"Unhandled document type '{documentType}'."),
        };
    }

    /// <summary>
    /// Attempts to parse machine-readable-zone text, splitting it into lines on line breaks.
    /// </summary>
    /// <param name="mrzText">The MRZ text, with lines separated by '\n', "\r\n", or '\r'.</param>
    /// <param name="document">
    /// The parsed document on success; <see langword="null"/> if <paramref name="mrzText"/> is
    /// not structurally valid MRZ text.
    /// </param>
    /// <returns><see langword="true"/> if parsing succeeded; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mrzText"/> is <see langword="null"/>.</exception>
    public static bool TryParse(string mrzText, out MrzDocument? document)
    {
        ArgumentNullException.ThrowIfNull(mrzText);
        return TryParse(SplitLines(mrzText), out document);
    }

    /// <summary>
    /// Attempts to parse machine-readable-zone text supplied as individual lines.
    /// </summary>
    /// <param name="lines">The MRZ lines, in document order.</param>
    /// <param name="document">
    /// The parsed document on success; <see langword="null"/> if <paramref name="lines"/> does
    /// not form a structurally valid MRZ layout.
    /// </param>
    /// <returns><see langword="true"/> if parsing succeeded; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="lines"/> is <see langword="null"/>.</exception>
    public static bool TryParse(IReadOnlyList<string> lines, out MrzDocument? document)
    {
        ArgumentNullException.ThrowIfNull(lines);

        try
        {
            document = Parse(lines);
            return true;
        }
        catch (MrzFormatException)
        {
            document = null;
            return false;
        }
    }

    private static IReadOnlyList<string> SplitLines(string mrzText)
    {
        string normalized = mrzText.Replace("\r\n", "\n").Replace('\r', '\n');
        string[] rawLines = normalized.Split('\n');

        List<string> lines = new(rawLines.Length);
        foreach (string rawLine in rawLines)
        {
            string trimmedLine = rawLine.Trim();
            if (trimmedLine.Length > 0)
            {
                lines.Add(trimmedLine);
            }
        }

        return lines;
    }

    private static void EnsureNoNullLines(IReadOnlyList<string> lines)
    {
        foreach (string? line in lines)
        {
            if (line is null)
            {
                throw new MrzFormatException("The MRZ line list contains a null entry.");
            }
        }
    }

    private static MrzDocumentType DetectDocumentType(IReadOnlyList<string> lines)
    {
        if (lines.Count == Td1Layout.LineCount && AllLinesHaveLength(lines, Td1Layout.LineLength))
        {
            return MrzDocumentType.Td1;
        }

        // TD2 IDs and MRV-B visas share the two-lines-of-36 geometry, and TD3 passports and
        // MRV-A visas share the two-lines-of-44 geometry. The only thing that distinguishes a
        // visa is a document code beginning with 'V' (ICAO Doc 9303 Part 7); anything else in
        // the code position keeps the existing TD2/TD3 ID or passport path unchanged. The gate
        // is deliberately strict on that first character.
        if (lines.Count == Td2Layout.LineCount && AllLinesHaveLength(lines, Td2Layout.LineLength))
        {
            return StartsWithVisaCode(lines[0], Td2Layout.DocumentCodeOffset)
                ? MrzDocumentType.MrvB
                : MrzDocumentType.Td2;
        }

        if (lines.Count == Td3Layout.LineCount && AllLinesHaveLength(lines, Td3Layout.LineLength))
        {
            return StartsWithVisaCode(lines[0], Td3Layout.DocumentCodeOffset)
                ? MrzDocumentType.MrvA
                : MrzDocumentType.Td3;
        }

        string lineLengths = string.Join(", ", lines.Select(line => line.Length));
        throw new MrzFormatException(
            $"Could not recognize an MRZ layout from {lines.Count} line(s) of length(s) [{lineLengths}]. "
            + $"Expected {Td1Layout.LineCount} lines of {Td1Layout.LineLength} (TD1), "
            + $"{Td2Layout.LineCount} lines of {Td2Layout.LineLength} (TD2), or "
            + $"{Td3Layout.LineCount} lines of {Td3Layout.LineLength} (TD3).");
    }

    private static bool AllLinesHaveLength(IReadOnlyList<string> lines, int expectedLength)
    {
        foreach (string line in lines)
        {
            if (line.Length != expectedLength)
            {
                return false;
            }
        }

        return true;
    }

    private static bool StartsWithVisaCode(string line1, int documentCodeOffset) =>
        line1[documentCodeOffset] == MrzConstants.VisaDocumentCodeCharacter;

    private static MrzDocument ParseTd3(IReadOnlyList<string> lines)
    {
        string line1 = lines[0];
        string line2 = lines[1];

        string documentCode = TrimTrailingFiller(ExtractLetters(line1, Td3Layout.DocumentCodeOffset, MrzConstants.DocumentCodeLength, "document code"));
        string issuingState = ExtractLetters(line1, Td3Layout.IssuingStateOffset, MrzConstants.StateOrNationalityLength, "issuing state");
        string nameField = ExtractLetters(line1, Td3Layout.NameFieldOffset, Td3Layout.NameFieldLength, "name field");

        string documentNumber = ExtractAlphanumeric(line2, Td3Layout.DocumentNumberOffset, MrzConstants.DocumentNumberLength, "document number");
        char documentNumberCheckDigit = ExtractCheckDigit(line2, Td3Layout.DocumentNumberCheckDigitOffset, "document number check digit");
        string nationality = ExtractLetters(line2, Td3Layout.NationalityOffset, MrzConstants.StateOrNationalityLength, "nationality");
        string dateOfBirth = ExtractDateOfBirth(line2, Td3Layout.DateOfBirthOffset, MrzConstants.DateLength, "date of birth");
        char dateOfBirthCheckDigit = ExtractCheckDigit(line2, Td3Layout.DateOfBirthCheckDigitOffset, "date of birth check digit");
        char sexCharacter = ExtractSex(line2, Td3Layout.SexOffset);
        string dateOfExpiry = ExtractDigits(line2, Td3Layout.DateOfExpiryOffset, MrzConstants.DateLength, "date of expiry");
        char dateOfExpiryCheckDigit = ExtractCheckDigit(line2, Td3Layout.DateOfExpiryCheckDigitOffset, "date of expiry check digit");
        string personalNumber = ExtractAlphanumeric(line2, Td3Layout.PersonalNumberOffset, Td3Layout.PersonalNumberLength, "personal number");
        char personalNumberCheckDigit = ExtractCheckDigit(line2, Td3Layout.PersonalNumberCheckDigitOffset, "personal number check digit");
        char compositeCheckDigit = ExtractCompositeCheckDigit(line2, Td3Layout.CompositeCheckDigitOffset, "composite check digit");

        (string surname, string givenNames, bool isTruncated) = MrzNameField.Parse(nameField);

        string compositeInput = string.Concat(
            line2.Substring(Td3Layout.CompositeInputStartOffset, Td3Layout.CompositeInputDocumentNumberSegmentLength),
            line2.Substring(Td3Layout.DateOfBirthOffset, Td3Layout.CompositeInputDateOfBirthSegmentLength),
            line2.Substring(Td3Layout.DateOfExpiryOffset, Td3Layout.CompositeInputTrailingSegmentLength));

        bool documentNumberValid = MrzCheckDigitCalculator.Verify(documentNumber, documentNumberCheckDigit);
        bool dateOfBirthValid = MrzCheckDigitCalculator.Verify(dateOfBirth, dateOfBirthCheckDigit);
        bool dateOfExpiryValid = MrzCheckDigitCalculator.Verify(dateOfExpiry, dateOfExpiryCheckDigit);
        bool personalNumberValid = MrzCheckDigitCalculator.Verify(personalNumber, personalNumberCheckDigit);
        bool compositeValid = MrzCheckDigitCalculator.Verify(compositeInput, compositeCheckDigit);

        MrzValidationResult validation = new(
            documentNumberValid,
            dateOfBirthValid,
            dateOfExpiryValid,
            personalNumberValid,
            compositeValid,
            documentNumberValid && dateOfBirthValid && dateOfExpiryValid && personalNumberValid && compositeValid);

        return new MrzDocument(
            MrzDocumentType.Td3,
            documentCode,
            issuingState,
            surname,
            givenNames,
            isTruncated,
            TrimTrailingFiller(documentNumber),
            nationality,
            dateOfBirth,
            MapSex(sexCharacter),
            dateOfExpiry,
            TrimToNull(personalNumber),
            null,
            lines,
            validation);
    }

    private static MrzDocument ParseMrvA(IReadOnlyList<string> lines)
    {
        string line1 = lines[0];
        string line2 = lines[1];

        string documentCode = TrimTrailingFiller(ExtractLetters(line1, MrvALayout.DocumentCodeOffset, MrzConstants.DocumentCodeLength, "document code"));
        string issuingState = ExtractLetters(line1, MrvALayout.IssuingStateOffset, MrzConstants.StateOrNationalityLength, "issuing state");
        string nameField = ExtractLetters(line1, MrvALayout.NameFieldOffset, MrvALayout.NameFieldLength, "name field");

        string documentNumber = ExtractAlphanumeric(line2, MrvALayout.DocumentNumberOffset, MrzConstants.DocumentNumberLength, "document number");
        char documentNumberCheckDigit = ExtractCheckDigit(line2, MrvALayout.DocumentNumberCheckDigitOffset, "document number check digit");
        string nationality = ExtractLetters(line2, MrvALayout.NationalityOffset, MrzConstants.StateOrNationalityLength, "nationality");
        string dateOfBirth = ExtractDateOfBirth(line2, MrvALayout.DateOfBirthOffset, MrzConstants.DateLength, "date of birth");
        char dateOfBirthCheckDigit = ExtractCheckDigit(line2, MrvALayout.DateOfBirthCheckDigitOffset, "date of birth check digit");
        char sexCharacter = ExtractSex(line2, MrvALayout.SexOffset);
        string dateOfExpiry = ExtractDigits(line2, MrvALayout.DateOfExpiryOffset, MrzConstants.DateLength, "date of expiry");
        char dateOfExpiryCheckDigit = ExtractCheckDigit(line2, MrvALayout.DateOfExpiryCheckDigitOffset, "date of expiry check digit");
        string optionalData = ExtractAlphanumeric(line2, MrvALayout.OptionalDataOffset, MrvALayout.OptionalDataLength, "optional data");

        return BuildVisa(MrzDocumentType.MrvA, lines, documentCode, issuingState, nameField, documentNumber,
            documentNumberCheckDigit, nationality, dateOfBirth, dateOfBirthCheckDigit, sexCharacter, dateOfExpiry,
            dateOfExpiryCheckDigit, optionalData);
    }

    private static MrzDocument ParseMrvB(IReadOnlyList<string> lines)
    {
        string line1 = lines[0];
        string line2 = lines[1];

        string documentCode = TrimTrailingFiller(ExtractLetters(line1, MrvBLayout.DocumentCodeOffset, MrzConstants.DocumentCodeLength, "document code"));
        string issuingState = ExtractLetters(line1, MrvBLayout.IssuingStateOffset, MrzConstants.StateOrNationalityLength, "issuing state");
        string nameField = ExtractLetters(line1, MrvBLayout.NameFieldOffset, MrvBLayout.NameFieldLength, "name field");

        string documentNumber = ExtractAlphanumeric(line2, MrvBLayout.DocumentNumberOffset, MrzConstants.DocumentNumberLength, "document number");
        char documentNumberCheckDigit = ExtractCheckDigit(line2, MrvBLayout.DocumentNumberCheckDigitOffset, "document number check digit");
        string nationality = ExtractLetters(line2, MrvBLayout.NationalityOffset, MrzConstants.StateOrNationalityLength, "nationality");
        string dateOfBirth = ExtractDateOfBirth(line2, MrvBLayout.DateOfBirthOffset, MrzConstants.DateLength, "date of birth");
        char dateOfBirthCheckDigit = ExtractCheckDigit(line2, MrvBLayout.DateOfBirthCheckDigitOffset, "date of birth check digit");
        char sexCharacter = ExtractSex(line2, MrvBLayout.SexOffset);
        string dateOfExpiry = ExtractDigits(line2, MrvBLayout.DateOfExpiryOffset, MrzConstants.DateLength, "date of expiry");
        char dateOfExpiryCheckDigit = ExtractCheckDigit(line2, MrvBLayout.DateOfExpiryCheckDigitOffset, "date of expiry check digit");
        string optionalData = ExtractAlphanumeric(line2, MrvBLayout.OptionalDataOffset, MrvBLayout.OptionalDataLength, "optional data");

        return BuildVisa(MrzDocumentType.MrvB, lines, documentCode, issuingState, nameField, documentNumber,
            documentNumberCheckDigit, nationality, dateOfBirth, dateOfBirthCheckDigit, sexCharacter, dateOfExpiry,
            dateOfExpiryCheckDigit, optionalData);
    }

    // MRV-A and MRV-B differ only in geometry (line length, name-field length, and the width of
    // the trailing optional-data field); once each has extracted its fields the assembly and
    // validation are identical, so they share this builder. Crucially, a machine-readable visa
    // has NO overall composite check digit and no independently check-digited personal number:
    // ICAO 9303 Part 7 defines the trailing line-2 region as plain optional data. The composite
    // computation that TD1/TD2/TD3 perform is therefore deliberately not run here, and both the
    // composite and personal-number validity flags are reported as null (not applicable) rather
    // than a misleading false.
    private static MrzDocument BuildVisa(
        MrzDocumentType documentType,
        IReadOnlyList<string> lines,
        string documentCode,
        string issuingState,
        string nameField,
        string documentNumber,
        char documentNumberCheckDigit,
        string nationality,
        string dateOfBirth,
        char dateOfBirthCheckDigit,
        char sexCharacter,
        string dateOfExpiry,
        char dateOfExpiryCheckDigit,
        string optionalData)
    {
        (string surname, string givenNames, bool isTruncated) = MrzNameField.Parse(nameField);

        bool documentNumberValid = MrzCheckDigitCalculator.Verify(documentNumber, documentNumberCheckDigit);
        bool dateOfBirthValid = MrzCheckDigitCalculator.Verify(dateOfBirth, dateOfBirthCheckDigit);
        bool dateOfExpiryValid = MrzCheckDigitCalculator.Verify(dateOfExpiry, dateOfExpiryCheckDigit);

        MrzValidationResult validation = new(
            documentNumberValid,
            dateOfBirthValid,
            dateOfExpiryValid,
            null,
            null,
            documentNumberValid && dateOfBirthValid && dateOfExpiryValid);

        return new MrzDocument(
            documentType,
            documentCode,
            issuingState,
            surname,
            givenNames,
            isTruncated,
            TrimTrailingFiller(documentNumber),
            nationality,
            dateOfBirth,
            MapSex(sexCharacter),
            dateOfExpiry,
            TrimToNull(optionalData),
            null,
            lines,
            validation);
    }

    private static MrzDocument ParseTd2(IReadOnlyList<string> lines)
    {
        string line1 = lines[0];
        string line2 = lines[1];

        string documentCode = TrimTrailingFiller(ExtractLetters(line1, Td2Layout.DocumentCodeOffset, MrzConstants.DocumentCodeLength, "document code"));
        string issuingState = ExtractLetters(line1, Td2Layout.IssuingStateOffset, MrzConstants.StateOrNationalityLength, "issuing state");
        string nameField = ExtractLetters(line1, Td2Layout.NameFieldOffset, Td2Layout.NameFieldLength, "name field");

        string documentNumber = ExtractAlphanumeric(line2, Td2Layout.DocumentNumberOffset, MrzConstants.DocumentNumberLength, "document number");
        char documentNumberCheckDigit = ExtractCheckDigit(line2, Td2Layout.DocumentNumberCheckDigitOffset, "document number check digit");
        string nationality = ExtractLetters(line2, Td2Layout.NationalityOffset, MrzConstants.StateOrNationalityLength, "nationality");
        string dateOfBirth = ExtractDateOfBirth(line2, Td2Layout.DateOfBirthOffset, MrzConstants.DateLength, "date of birth");
        char dateOfBirthCheckDigit = ExtractCheckDigit(line2, Td2Layout.DateOfBirthCheckDigitOffset, "date of birth check digit");
        char sexCharacter = ExtractSex(line2, Td2Layout.SexOffset);
        string dateOfExpiry = ExtractDigits(line2, Td2Layout.DateOfExpiryOffset, MrzConstants.DateLength, "date of expiry");
        char dateOfExpiryCheckDigit = ExtractCheckDigit(line2, Td2Layout.DateOfExpiryCheckDigitOffset, "date of expiry check digit");
        string optionalData = ExtractAlphanumeric(line2, Td2Layout.OptionalDataOffset, Td2Layout.OptionalDataLength, "optional data");
        char compositeCheckDigit = ExtractCompositeCheckDigit(line2, Td2Layout.CompositeCheckDigitOffset, "composite check digit");

        (string surname, string givenNames, bool isTruncated) = MrzNameField.Parse(nameField);

        string compositeInput = string.Concat(
            line2.Substring(Td2Layout.CompositeInputStartOffset, Td2Layout.CompositeInputDocumentNumberSegmentLength),
            line2.Substring(Td2Layout.DateOfBirthOffset, Td2Layout.CompositeInputDateOfBirthSegmentLength),
            line2.Substring(Td2Layout.DateOfExpiryOffset, Td2Layout.CompositeInputTrailingSegmentLength));

        bool documentNumberValid = MrzCheckDigitCalculator.Verify(documentNumber, documentNumberCheckDigit);
        bool dateOfBirthValid = MrzCheckDigitCalculator.Verify(dateOfBirth, dateOfBirthCheckDigit);
        bool dateOfExpiryValid = MrzCheckDigitCalculator.Verify(dateOfExpiry, dateOfExpiryCheckDigit);
        bool compositeValid = MrzCheckDigitCalculator.Verify(compositeInput, compositeCheckDigit);

        MrzValidationResult validation = new(
            documentNumberValid,
            dateOfBirthValid,
            dateOfExpiryValid,
            null,
            compositeValid,
            documentNumberValid && dateOfBirthValid && dateOfExpiryValid && compositeValid);

        return new MrzDocument(
            MrzDocumentType.Td2,
            documentCode,
            issuingState,
            surname,
            givenNames,
            isTruncated,
            TrimTrailingFiller(documentNumber),
            nationality,
            dateOfBirth,
            MapSex(sexCharacter),
            dateOfExpiry,
            TrimToNull(optionalData),
            null,
            lines,
            validation);
    }

    private static MrzDocument ParseTd1(IReadOnlyList<string> lines)
    {
        string line1 = lines[0];
        string line2 = lines[1];
        string line3 = lines[2];

        string documentCode = TrimTrailingFiller(ExtractLetters(line1, Td1Layout.DocumentCodeOffset, MrzConstants.DocumentCodeLength, "document code"));
        string issuingState = ExtractLetters(line1, Td1Layout.IssuingStateOffset, MrzConstants.StateOrNationalityLength, "issuing state");
        string documentNumber = ExtractAlphanumeric(line1, Td1Layout.DocumentNumberOffset, MrzConstants.DocumentNumberLength, "document number");
        char documentNumberCheckDigit = ExtractCheckDigit(line1, Td1Layout.DocumentNumberCheckDigitOffset, "document number check digit");
        string optionalData1 = ExtractAlphanumeric(line1, Td1Layout.OptionalData1Offset, Td1Layout.OptionalData1Length, "optional data 1");

        string dateOfBirth = ExtractDateOfBirth(line2, Td1Layout.DateOfBirthOffset, MrzConstants.DateLength, "date of birth");
        char dateOfBirthCheckDigit = ExtractCheckDigit(line2, Td1Layout.DateOfBirthCheckDigitOffset, "date of birth check digit");
        char sexCharacter = ExtractSex(line2, Td1Layout.SexOffset);
        string dateOfExpiry = ExtractDigits(line2, Td1Layout.DateOfExpiryOffset, MrzConstants.DateLength, "date of expiry");
        char dateOfExpiryCheckDigit = ExtractCheckDigit(line2, Td1Layout.DateOfExpiryCheckDigitOffset, "date of expiry check digit");
        string nationality = ExtractLetters(line2, Td1Layout.NationalityOffset, MrzConstants.StateOrNationalityLength, "nationality");
        string optionalData2 = ExtractAlphanumeric(line2, Td1Layout.OptionalData2Offset, Td1Layout.OptionalData2Length, "optional data 2");
        char compositeCheckDigit = ExtractCompositeCheckDigit(line2, Td1Layout.CompositeCheckDigitOffset, "composite check digit");

        string nameField = ExtractLetters(line3, Td1Layout.NameFieldOffset, Td1Layout.NameFieldLength, "name field");
        (string surname, string givenNames, bool isTruncated) = MrzNameField.Parse(nameField);

        string compositeInput = string.Concat(
            line1.Substring(Td1Layout.CompositeInputLine1StartOffset, Td1Layout.CompositeInputLine1SegmentLength),
            line2.Substring(Td1Layout.DateOfBirthOffset, Td1Layout.CompositeInputDateOfBirthSegmentLength),
            line2.Substring(Td1Layout.DateOfExpiryOffset, Td1Layout.CompositeInputDateOfExpirySegmentLength),
            optionalData2);

        bool documentNumberValid = MrzCheckDigitCalculator.Verify(documentNumber, documentNumberCheckDigit);
        bool dateOfBirthValid = MrzCheckDigitCalculator.Verify(dateOfBirth, dateOfBirthCheckDigit);
        bool dateOfExpiryValid = MrzCheckDigitCalculator.Verify(dateOfExpiry, dateOfExpiryCheckDigit);
        bool compositeValid = MrzCheckDigitCalculator.Verify(compositeInput, compositeCheckDigit);

        MrzValidationResult validation = new(
            documentNumberValid,
            dateOfBirthValid,
            dateOfExpiryValid,
            null,
            compositeValid,
            documentNumberValid && dateOfBirthValid && dateOfExpiryValid && compositeValid);

        return new MrzDocument(
            MrzDocumentType.Td1,
            documentCode,
            issuingState,
            surname,
            givenNames,
            isTruncated,
            TrimTrailingFiller(documentNumber),
            nationality,
            dateOfBirth,
            MapSex(sexCharacter),
            dateOfExpiry,
            TrimToNull(optionalData2),
            TrimToNull(optionalData1),
            lines,
            validation);
    }

    private static string ExtractLetters(string line, int offset, int length, string fieldName)
    {
        string field = line.Substring(offset, length);
        MrzCharsetValidator.EnsureLettersOrFiller(field, fieldName);
        return field;
    }

    private static string ExtractAlphanumeric(string line, int offset, int length, string fieldName)
    {
        string field = line.Substring(offset, length);
        MrzCharsetValidator.EnsureAlphanumericOrFiller(field, fieldName);
        return field;
    }

    private static string ExtractDigits(string line, int offset, int length, string fieldName)
    {
        string field = line.Substring(offset, length);
        MrzCharsetValidator.EnsureDigits(field, fieldName);
        return field;
    }

    private static string ExtractDateOfBirth(string line, int offset, int length, string fieldName)
    {
        string field = line.Substring(offset, length);
        MrzCharsetValidator.EnsureDigitsOrFiller(field, fieldName);
        return field;
    }

    private static char ExtractCheckDigit(string line, int offset, string fieldName)
    {
        char checkDigit = line[offset];
        MrzCharsetValidator.EnsureCheckDigitCharacter(checkDigit, fieldName);
        return checkDigit;
    }

    private static char ExtractCompositeCheckDigit(string line, int offset, string fieldName)
    {
        char checkDigit = line[offset];
        MrzCharsetValidator.EnsureCompositeCheckDigitCharacter(checkDigit, fieldName);
        return checkDigit;
    }

    private static char ExtractSex(string line, int offset)
    {
        char sexCharacter = line[offset];
        MrzCharsetValidator.EnsureSexCharacter(sexCharacter);
        return sexCharacter;
    }

    private static MrzSex MapSex(char sexCharacter) => sexCharacter switch
    {
        MrzConstants.SexMaleCharacter => MrzSex.Male,
        MrzConstants.SexFemaleCharacter => MrzSex.Female,
        // Filler ('<') is the ICAO-conformant "unspecified" marker; 'X' is a nonconformant
        // marker EnsureSexCharacter also permits, mapped to the same value.
        _ => MrzSex.Unspecified,
    };

    private static string TrimTrailingFiller(string field) => field.TrimEnd(MrzConstants.FillerCharacter);

    private static string? TrimToNull(string field)
    {
        string trimmed = TrimTrailingFiller(field);
        return trimmed.Length == 0 ? null : trimmed;
    }
}
