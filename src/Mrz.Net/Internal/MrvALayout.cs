namespace Mrz.Internal;

/// <summary>
/// Field positions for the MRV-A format: a machine-readable visa in two lines of 44 characters
/// (ICAO Doc 9303 Part 7). Line 1 and the leading fields of line 2 share the TD3 geometry, but
/// the trailing region of line 2 is a single optional-data field: an MRV carries no personal
/// number check digit and no overall composite check digit.
/// </summary>
internal static class MrvALayout
{
    /// <summary>The fixed length, in characters, of each MRV-A line.</summary>
    internal const int LineLength = 44;

    /// <summary>The number of lines an MRV-A machine-readable zone occupies.</summary>
    internal const int LineCount = 2;

    /// <summary>The length, in characters, of the name field on line 1.</summary>
    internal const int NameFieldLength = LineLength - MrzConstants.DocumentCodeLength - MrzConstants.StateOrNationalityLength;

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

    /// <summary>The offset, on line 2, of the optional-data field.</summary>
    internal const int OptionalDataOffset = DateOfExpiryCheckDigitOffset + MrzConstants.CheckDigitLength;

    /// <summary>
    /// The length, in characters, of the optional-data field on line 2: the remainder of the
    /// line after the date-of-expiry check digit. Unlike TD3, there is no personal number
    /// check digit and no composite check digit occupying part of this region.
    /// </summary>
    internal const int OptionalDataLength = LineLength - OptionalDataOffset;
}
