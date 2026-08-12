namespace Mrz.Internal;

/// <summary>
/// Validates that MRZ fields use only the character classes ICAO 9303 permits in each
/// position, so malformed or misread input fails fast with a descriptive error instead of
/// producing a silently wrong parse.
/// </summary>
internal static class MrzCharsetValidator
{
    /// <summary>
    /// Ensures every character in <paramref name="field"/> is an uppercase letter A-Z or the
    /// filler character, as required for document codes, state and nationality codes, and name
    /// fields.
    /// </summary>
    /// <param name="field">The field text to validate.</param>
    /// <param name="fieldName">The field name to use in the exception message.</param>
    /// <exception cref="MrzFormatException"><paramref name="field"/> contains a disallowed character.</exception>
    internal static void EnsureLettersOrFiller(ReadOnlySpan<char> field, string fieldName)
    {
        foreach (char character in field)
        {
            bool isLetter = character is >= MrzConstants.LetterA and <= MrzConstants.LetterZ;
            bool isFiller = character == MrzConstants.FillerCharacter;

            if (!isLetter && !isFiller)
            {
                throw new MrzFormatException(
                    $"Field '{fieldName}' contains '{character}', which is not an uppercase letter or filler character.");
            }
        }
    }

    /// <summary>
    /// Ensures every character in <paramref name="field"/> is a digit, an uppercase letter A-Z,
    /// or the filler character, as required for document number and optional/personal number
    /// fields.
    /// </summary>
    /// <param name="field">The field text to validate.</param>
    /// <param name="fieldName">The field name to use in the exception message.</param>
    /// <exception cref="MrzFormatException"><paramref name="field"/> contains a disallowed character.</exception>
    internal static void EnsureAlphanumericOrFiller(ReadOnlySpan<char> field, string fieldName)
    {
        foreach (char character in field)
        {
            bool isDigit = character is >= MrzConstants.DigitZero and <= MrzConstants.DigitNine;
            bool isLetter = character is >= MrzConstants.LetterA and <= MrzConstants.LetterZ;
            bool isFiller = character == MrzConstants.FillerCharacter;

            if (!isDigit && !isLetter && !isFiller)
            {
                throw new MrzFormatException(
                    $"Field '{fieldName}' contains '{character}', which is not a digit, uppercase letter, or filler character.");
            }
        }
    }

    /// <summary>
    /// Ensures every character in <paramref name="field"/> is a digit, as required for
    /// date-of-birth and date-of-expiry fields.
    /// </summary>
    /// <param name="field">The field text to validate.</param>
    /// <param name="fieldName">The field name to use in the exception message.</param>
    /// <exception cref="MrzFormatException"><paramref name="field"/> contains a non-digit character.</exception>
    internal static void EnsureDigits(ReadOnlySpan<char> field, string fieldName)
    {
        foreach (char character in field)
        {
            if (character is < MrzConstants.DigitZero or > MrzConstants.DigitNine)
            {
                throw new MrzFormatException($"Field '{fieldName}' contains '{character}', which is not a digit.");
            }
        }
    }

    /// <summary>
    /// Ensures every character in <paramref name="field"/> is a digit or the filler character, as
    /// required for the date-of-birth field. ICAO 9303 permits the filler character in date-of-birth
    /// positions whose value is unknown to the issuing authority, so a partially or entirely
    /// unknown birth date is not a structural error the way a non-digit, non-filler character
    /// would be.
    /// </summary>
    /// <param name="field">The field text to validate.</param>
    /// <param name="fieldName">The field name to use in the exception message.</param>
    /// <exception cref="MrzFormatException"><paramref name="field"/> contains a disallowed character.</exception>
    internal static void EnsureDigitsOrFiller(ReadOnlySpan<char> field, string fieldName)
    {
        foreach (char character in field)
        {
            bool isDigit = character is >= MrzConstants.DigitZero and <= MrzConstants.DigitNine;
            bool isFiller = character == MrzConstants.FillerCharacter;

            if (!isDigit && !isFiller)
            {
                throw new MrzFormatException(
                    $"Field '{fieldName}' contains '{character}', which is not a digit or filler character.");
            }
        }
    }

    /// <summary>
    /// Ensures a check-digit character is a digit or the filler character.
    /// </summary>
    /// <param name="checkDigit">The character occupying a check-digit position.</param>
    /// <param name="fieldName">The field name to use in the exception message.</param>
    /// <exception cref="MrzFormatException"><paramref name="checkDigit"/> is neither a digit nor filler.</exception>
    internal static void EnsureCheckDigitCharacter(char checkDigit, string fieldName)
    {
        bool isDigit = checkDigit is >= MrzConstants.DigitZero and <= MrzConstants.DigitNine;
        bool isFiller = checkDigit == MrzConstants.FillerCharacter;

        if (!isDigit && !isFiller)
        {
            throw new MrzFormatException(
                $"Check digit '{fieldName}' is '{checkDigit}', which is not a digit or filler character.");
        }
    }

    /// <summary>
    /// Ensures the composite check-digit character is a digit. Unlike the per-field check digits,
    /// ICAO 9303 never permits the filler character here: the composite check digit protects a
    /// concatenation of several fields, and there is no "unused field" case a filler could
    /// legitimately signal, so a filler (or any other non-digit) in this position is treated as a
    /// structural error rather than a data-quality one.
    /// </summary>
    /// <param name="checkDigit">The character occupying the composite check-digit position.</param>
    /// <param name="fieldName">The field name to use in the exception message.</param>
    /// <exception cref="MrzFormatException"><paramref name="checkDigit"/> is not a digit.</exception>
    internal static void EnsureCompositeCheckDigitCharacter(char checkDigit, string fieldName)
    {
        bool isDigit = checkDigit is >= MrzConstants.DigitZero and <= MrzConstants.DigitNine;

        if (!isDigit)
        {
            throw new MrzFormatException(
                $"Check digit '{fieldName}' is '{checkDigit}', which is not a digit. ICAO 9303 requires the "
                + "composite check digit to be a digit even in positions where other check digits permit filler.");
        }
    }

    /// <summary>
    /// Ensures a sex-marker character is 'M', 'F', the filler character, or 'X'.
    /// </summary>
    /// <param name="sexCharacter">The character occupying the sex position.</param>
    /// <remarks>
    /// ICAO 9303 itself only defines 'M', 'F', and the filler character for this position. 'X' is
    /// accepted here as a deliberate leniency because it has been observed in real-world,
    /// nonconformant machine-readable zones; it maps to <see cref="Mrz.MrzSex.Unspecified"/>, the
    /// same as the filler character.
    /// </remarks>
    /// <exception cref="MrzFormatException"><paramref name="sexCharacter"/> is none of the permitted markers.</exception>
    internal static void EnsureSexCharacter(char sexCharacter)
    {
        bool isPermitted = sexCharacter is MrzConstants.SexMaleCharacter or MrzConstants.SexFemaleCharacter
            or MrzConstants.FillerCharacter or MrzConstants.SexUnspecifiedCharacter;

        if (!isPermitted)
        {
            throw new MrzFormatException(
                $"Sex marker '{sexCharacter}' is not one of the permitted characters 'M', 'F', 'X', or '<'.");
        }
    }
}
