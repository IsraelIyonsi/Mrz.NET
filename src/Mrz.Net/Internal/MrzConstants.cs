namespace Mrz.Internal;

/// <summary>
/// Shared constants for ICAO 9303 machine-readable-zone parsing and check-digit computation.
/// </summary>
internal static class MrzConstants
{
    /// <summary>
    /// The filler character used to pad unused positions and to separate name components.
    /// </summary>
    internal const char FillerCharacter = '<';

    /// <summary>
    /// The two-character sequence that separates the primary identifier (surname) from the
    /// secondary identifier (given names) within a name field.
    /// </summary>
    internal const string NameComponentSeparator = "<<";

    /// <summary>
    /// The repeating 7-3-1 weighting sequence used by the ICAO 9303 check-digit algorithm.
    /// </summary>
    internal static readonly int[] CheckDigitWeights = { 7, 3, 1 };

    /// <summary>
    /// The modulus applied to the weighted sum to derive a single check digit.
    /// </summary>
    internal const int CheckDigitModulus = 10;

    /// <summary>
    /// The character value assigned to the digit '0', used as the base for numeric characters.
    /// </summary>
    internal const char DigitZero = '0';

    /// <summary>
    /// The character value assigned to the digit '9', the upper bound of numeric characters.
    /// </summary>
    internal const char DigitNine = '9';

    /// <summary>
    /// The character value assigned to the letter 'A', the lower bound of alphabetic characters.
    /// </summary>
    internal const char LetterA = 'A';

    /// <summary>
    /// The character value assigned to the letter 'Z', the upper bound of alphabetic characters.
    /// </summary>
    internal const char LetterZ = 'Z';

    /// <summary>
    /// The numeric value assigned to letter 'A' under the ICAO 9303 character-to-value mapping.
    /// </summary>
    internal const int LetterValueBase = 10;

    /// <summary>
    /// The length, in characters, of every check-digit field in the machine-readable zone.
    /// </summary>
    internal const int CheckDigitLength = 1;

    /// <summary>
    /// The length, in characters, of a date field expressed as YYMMDD.
    /// </summary>
    internal const int DateLength = 6;

    /// <summary>
    /// The length, in characters, of the sex field.
    /// </summary>
    internal const int SexLength = 1;

    /// <summary>
    /// The length, in characters, of a two-letter document code field.
    /// </summary>
    internal const int DocumentCodeLength = 2;

    /// <summary>
    /// The length, in characters, of a three-letter issuing-state or nationality field.
    /// </summary>
    internal const int StateOrNationalityLength = 3;

    /// <summary>
    /// The length, in characters, of the fixed-width document number field.
    /// </summary>
    internal const int DocumentNumberLength = 9;

    /// <summary>
    /// The character representing the male sex marker in the sex field.
    /// </summary>
    internal const char SexMaleCharacter = 'M';

    /// <summary>
    /// The character representing the female sex marker in the sex field.
    /// </summary>
    internal const char SexFemaleCharacter = 'F';

    /// <summary>
    /// The character some nonconformant real-world documents use in the sex field to indicate an
    /// unspecified or non-binary sex. ICAO 9303 itself only defines 'M', 'F', and the filler
    /// character for this position, but 'X' is accepted here (mapped to
    /// <see cref="Mrz.MrzSex.Unspecified"/>) so such documents parse instead of failing
    /// structurally.
    /// </summary>
    internal const char SexUnspecifiedCharacter = 'X';
}
