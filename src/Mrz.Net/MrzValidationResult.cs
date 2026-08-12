namespace Mrz;

/// <summary>
/// The outcome of verifying every ICAO 9303 check digit present in a parsed
/// <see cref="MrzDocument"/>.
/// </summary>
/// <param name="DocumentNumberCheckDigitValid">
/// Whether the document number check digit matches the computed 7-3-1 check digit.
/// </param>
/// <param name="DateOfBirthCheckDigitValid">
/// Whether the date-of-birth check digit matches the computed 7-3-1 check digit.
/// </param>
/// <param name="DateOfExpiryCheckDigitValid">
/// Whether the date-of-expiry check digit matches the computed 7-3-1 check digit.
/// </param>
/// <param name="PersonalNumberCheckDigitValid">
/// Whether the personal number check digit matches the computed 7-3-1 check digit, or
/// <see langword="null"/> for document types (TD1, TD2) that carry no independently
/// check-digited personal number field.
/// </param>
/// <param name="CompositeCheckDigitValid">
/// Whether the composite check digit matches the computed 7-3-1 check digit over the
/// concatenation of the fields it protects.
/// </param>
/// <param name="IsValid">
/// Whether every applicable check digit above is valid. This is the single flag most callers
/// need to decide whether an MRZ read is trustworthy.
/// </param>
public sealed record MrzValidationResult(
    bool DocumentNumberCheckDigitValid,
    bool DateOfBirthCheckDigitValid,
    bool DateOfExpiryCheckDigitValid,
    bool? PersonalNumberCheckDigitValid,
    bool CompositeCheckDigitValid,
    bool IsValid);
