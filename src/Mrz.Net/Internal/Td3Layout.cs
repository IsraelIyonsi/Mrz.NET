namespace Mrz.Internal;

/// <summary>
/// Field positions for the TD3 format: two lines of 44 characters, used by passports
/// (ICAO Doc 9303 Part 4).
/// </summary>
internal static class Td3Layout
{
    /// <summary>The fixed length, in characters, of each TD3 line.</summary>
    internal const int LineLength = 44;

    /// <summary>The number of lines a TD3 machine-readable zone occupies.</summary>
    internal const int LineCount = 2;

    /// <summary>The length, in characters, of the name field on line 1.</summary>
    internal const int NameFieldLength = LineLength - MrzConstants.DocumentCodeLength - MrzConstants.StateOrNationalityLength;

    /// <summary>The length, in characters, of the personal number field on line 2.</summary>
    internal const int PersonalNumberLength = 14;

    // Line 1 offsets.

    /// <summary>The offset, on line 1, of the two-character document code.</summary>
    internal const int DocumentCodeOffset = 0;

    /// <summary>The offset, on line 1, of the three-letter issuing-state code.</summary>
    internal const int IssuingStateOffset = DocumentCodeOffset + MrzConstants.DocumentCodeLength;

    /// <summary>The offset, on line 1, of the name field.</summary>
    internal const int NameFieldOffset = IssuingStateOffset + MrzConstants.StateOrNationalityLength;

    // Line 2 offsets.

    /// <summary>The offset, on line 2, of the document number.</summary>
    internal const int DocumentNumberOffset = 0;

    /// <summary>The offset, on line 2, of the document number check digit.</summary>
    internal const int DocumentNumberCheckDigitOffset = DocumentNumberOffset + MrzConstants.DocumentNumberLength;

    /// <summary>The offset, on line 2, of the three-letter nationality code.</summary>
    internal const int NationalityOffset = DocumentNumberCheckDigitOffset + MrzConstants.CheckDigitLength;

    /// <summary>The offset, on line 2, of the date-of-birth field.</summary>
    internal const int DateOfBirthOffset = NationalityOffset + MrzConstants.StateOrNationalityLength;

    /// <summary>The offset, on line 2, of the date-of-birth check digit.</summary>
    internal const int DateOfBirthCheckDigitOffset = DateOfBirthOffset + MrzConstants.DateLength;

    /// <summary>The offset, on line 2, of the sex marker.</summary>
    internal const int SexOffset = DateOfBirthCheckDigitOffset + MrzConstants.CheckDigitLength;

    /// <summary>The offset, on line 2, of the date-of-expiry field.</summary>
    internal const int DateOfExpiryOffset = SexOffset + MrzConstants.SexLength;

    /// <summary>The offset, on line 2, of the date-of-expiry check digit.</summary>
    internal const int DateOfExpiryCheckDigitOffset = DateOfExpiryOffset + MrzConstants.DateLength;

    /// <summary>The offset, on line 2, of the personal number field.</summary>
    internal const int PersonalNumberOffset = DateOfExpiryCheckDigitOffset + MrzConstants.CheckDigitLength;

    /// <summary>The offset, on line 2, of the personal number check digit.</summary>
    internal const int PersonalNumberCheckDigitOffset = PersonalNumberOffset + PersonalNumberLength;

    /// <summary>The offset, on line 2, of the composite check digit.</summary>
    internal const int CompositeCheckDigitOffset = PersonalNumberCheckDigitOffset + MrzConstants.CheckDigitLength;

    /// <summary>The offset, on line 2, where the composite check digit input begins.</summary>
    internal const int CompositeInputStartOffset = DocumentNumberOffset;

    /// <summary>The length, in characters, of the document-number segment of the composite input.</summary>
    internal const int CompositeInputDocumentNumberSegmentLength = MrzConstants.DocumentNumberLength + MrzConstants.CheckDigitLength;

    /// <summary>The length, in characters, of the date-of-birth segment of the composite input.</summary>
    internal const int CompositeInputDateOfBirthSegmentLength = MrzConstants.DateLength + MrzConstants.CheckDigitLength;

    /// <summary>The length, in characters, of the trailing segment (expiry through personal number) of the composite input.</summary>
    internal const int CompositeInputTrailingSegmentLength =
        MrzConstants.DateLength + MrzConstants.CheckDigitLength + PersonalNumberLength + MrzConstants.CheckDigitLength;
}
