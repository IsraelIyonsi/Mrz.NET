namespace Mrz;

/// <summary>
/// The fields parsed from an ICAO 9303 machine-readable zone, together with the outcome of
/// verifying its check digits.
/// </summary>
/// <param name="DocumentType">The MRZ size and layout the document uses (TD1, TD2, or TD3).</param>
/// <param name="DocumentCode">
/// The one- or two-letter document code from line 1 (for example "P" for a passport or "I" for
/// an ID card), with any filler padding removed.
/// </param>
/// <param name="IssuingState">The three-letter code of the state or organization that issued the document.</param>
/// <param name="Surname">
/// The primary identifier (surname) from the name field, with single fillers resolved to
/// spaces.
/// </param>
/// <param name="GivenNames">
/// The secondary identifier (given names) from the name field, with single fillers resolved to
/// spaces. Empty when the document records no given names.
/// </param>
/// <param name="IsNameTruncated">
/// Whether the name field ends without filler padding, which ICAO 9303 uses as the signal that
/// the printed identifiers filled the field exactly and may have been truncated to fit.
/// </param>
/// <param name="DocumentNumber">
/// The document number, with trailing filler padding removed. This is the 9-character field
/// only: Mrz.NET does not implement the ICAO 9303 extended document number mechanism (where a
/// document number longer than 9 characters overflows into the adjacent optional data field,
/// signaled by a filler in the check-digit position), so a TD1 or TD2 document using it will
/// parse with a truncated <see cref="DocumentNumber"/> and a failing document-number check
/// digit rather than being detected or rejected. See the README for details.
/// </param>
/// <param name="Nationality">The three-letter nationality code of the document holder.</param>
/// <param name="DateOfBirth">
/// The date of birth exactly as printed, six digits in YYMMDD form. ICAO 9303 does not define
/// how to resolve the two-digit year to a century, so no century inference is performed.
/// </param>
/// <param name="Sex">The sex marker from the MRZ.</param>
/// <param name="DateOfExpiry">
/// The date of expiry exactly as printed, six digits in YYMMDD form, subject to the same
/// century caveat as <paramref name="DateOfBirth"/>.
/// </param>
/// <param name="PersonalNumber">
/// The optional or personal number field (TD3 personal number, TD2 optional data, TD1 second
/// optional data field, or the MRV-A and MRV-B visa optional-data region), with trailing filler
/// padding removed, or <see langword="null"/> if the field is entirely filler. For a visa this
/// carries the raw optional data as printed; ICAO 9303 defines no check digit over it.
/// </param>
/// <param name="SupplementalOptionalData">
/// The TD1 first optional data field, adjacent to the document number on line 1, with trailing
/// filler padding removed, or <see langword="null"/> if the field is entirely filler or the
/// document is not a TD1. TD2 and TD3 carry no equivalent field.
/// </param>
/// <param name="Lines">The raw MRZ lines as parsed, unmodified.</param>
/// <param name="Validation">The outcome of verifying every check digit present in the document.</param>
public sealed record MrzDocument(
    MrzDocumentType DocumentType,
    string DocumentCode,
    string IssuingState,
    string Surname,
    string GivenNames,
    bool IsNameTruncated,
    string DocumentNumber,
    string Nationality,
    string DateOfBirth,
    MrzSex Sex,
    string DateOfExpiry,
    string? PersonalNumber,
    string? SupplementalOptionalData,
    IReadOnlyList<string> Lines,
    MrzValidationResult Validation);
