namespace Mrz.Internal;

/// <summary>
/// Field positions for the TD1 format: three lines of 30 characters, used by ID cards
/// (ICAO Doc 9303 Part 5).
/// </summary>
internal static class Td1Layout
{
    /// <summary>The fixed length, in characters, of each TD1 line.</summary>
    internal const int LineLength = 30;

    /// <summary>The number of lines a TD1 machine-readable zone occupies.</summary>
    internal const int LineCount = 3;

    /// <summary>The length, in characters, of the first optional data field on line 1.</summary>
    internal const int OptionalData1Length =
        LineLength - MrzConstants.DocumentCodeLength - MrzConstants.StateOrNationalityLength
        - MrzConstants.DocumentNumberLength - MrzConstants.CheckDigitLength;

    /// <summary>The length, in characters, of the second optional data field on line 2.</summary>
    internal const int OptionalData2Length =
        LineLength - MrzConstants.DateLength - MrzConstants.CheckDigitLength - MrzConstants.SexLength
        - MrzConstants.DateLength - MrzConstants.CheckDigitLength - MrzConstants.StateOrNationalityLength
        - MrzConstants.CheckDigitLength;

    // Line 1 offsets.

    /// <summary>The offset, on line 1, of the two-character document code.</summary>
    internal const int DocumentCodeOffset = 0;

    /// <summary>The offset, on line 1, of the three-letter issuing-state code.</summary>
    internal const int IssuingStateOffset = DocumentCodeOffset + MrzConstants.DocumentCodeLength;

    /// <summary>The offset, on line 1, of the document number.</summary>
    internal const int DocumentNumberOffset = IssuingStateOffset + MrzConstants.StateOrNationalityLength;

    /// <summary>The offset, on line 1, of the document number check digit.</summary>
    internal const int DocumentNumberCheckDigitOffset = DocumentNumberOffset + MrzConstants.DocumentNumberLength;

    /// <summary>The offset, on line 1, of the first optional data field.</summary>
    internal const int OptionalData1Offset = DocumentNumberCheckDigitOffset + MrzConstants.CheckDigitLength;

    // Line 2 offsets.

    /// <summary>The offset, on line 2, of the date-of-birth field.</summary>
    internal const int DateOfBirthOffset = 0;

    /// <summary>The offset, on line 2, of the date-of-birth check digit.</summary>
    internal const int DateOfBirthCheckDigitOffset = DateOfBirthOffset + MrzConstants.DateLength;

    /// <summary>The offset, on line 2, of the sex marker.</summary>
    internal const int SexOffset = DateOfBirthCheckDigitOffset + MrzConstants.CheckDigitLength;

    /// <summary>The offset, on line 2, of the date-of-expiry field.</summary>
    internal const int DateOfExpiryOffset = SexOffset + MrzConstants.SexLength;

    /// <summary>The offset, on line 2, of the date-of-expiry check digit.</summary>
    internal const int DateOfExpiryCheckDigitOffset = DateOfExpiryOffset + MrzConstants.DateLength;

    /// <summary>The offset, on line 2, of the three-letter nationality code.</summary>
    internal const int NationalityOffset = DateOfExpiryCheckDigitOffset + MrzConstants.CheckDigitLength;

    /// <summary>The offset, on line 2, of the second optional data field.</summary>
    internal const int OptionalData2Offset = NationalityOffset + MrzConstants.StateOrNationalityLength;

    /// <summary>The offset, on line 2, of the composite check digit.</summary>
    internal const int CompositeCheckDigitOffset = OptionalData2Offset + OptionalData2Length;

    // Line 3 offset.

    /// <summary>The offset, on line 3, of the name field.</summary>
    internal const int NameFieldOffset = 0;

    /// <summary>The length, in characters, of the name field on line 3.</summary>
    internal const int NameFieldLength = LineLength;

    // Composite check digit input, drawn from line 1 and line 2.

    /// <summary>The offset, on line 1, where the composite check digit input begins.</summary>
    internal const int CompositeInputLine1StartOffset = DocumentNumberOffset;

    /// <summary>The length, in characters, of the line-1 segment of the composite input.</summary>
    internal const int CompositeInputLine1SegmentLength = MrzConstants.DocumentNumberLength + MrzConstants.CheckDigitLength + OptionalData1Length;

    /// <summary>The length, in characters, of the date-of-birth segment of the composite input.</summary>
    internal const int CompositeInputDateOfBirthSegmentLength = MrzConstants.DateLength + MrzConstants.CheckDigitLength;

    /// <summary>The length, in characters, of the date-of-expiry segment of the composite input.</summary>
    internal const int CompositeInputDateOfExpirySegmentLength = MrzConstants.DateLength + MrzConstants.CheckDigitLength;
}
