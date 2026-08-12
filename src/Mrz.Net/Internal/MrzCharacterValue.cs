namespace Mrz.Internal;

/// <summary>
/// The ICAO 9303 character-to-value mapping used by the check-digit algorithm: digits keep
/// their numeric value, letters A-Z map to 10-35, and the filler character maps to 0.
/// </summary>
internal static class MrzCharacterValue
{
    /// <summary>
    /// Returns the numeric value of an MRZ character under the ICAO 9303 mapping.
    /// </summary>
    /// <param name="character">A single character from the machine-readable zone.</param>
    /// <returns>0-9 for a digit, 10-35 for a letter A-Z, or 0 for the filler character.</returns>
    /// <exception cref="MrzFormatException">
    /// <paramref name="character"/> is not a digit, an uppercase letter A-Z, or the filler
    /// character.
    /// </exception>
    internal static int GetValue(char character)
    {
        if (character is >= MrzConstants.DigitZero and <= MrzConstants.DigitNine)
        {
            return character - MrzConstants.DigitZero;
        }

        if (character is >= MrzConstants.LetterA and <= MrzConstants.LetterZ)
        {
            return character - MrzConstants.LetterA + MrzConstants.LetterValueBase;
        }

        if (character == MrzConstants.FillerCharacter)
        {
            return 0;
        }

        throw new MrzFormatException(
            $"'{character}' is not a valid MRZ character. Only digits, uppercase A-Z, and the filler character '<' are permitted.");
    }

    /// <summary>
    /// Reports whether every character in <paramref name="field"/> is the filler character,
    /// meaning the field is entirely unused.
    /// </summary>
    /// <param name="field">The field text to inspect.</param>
    /// <returns><see langword="true"/> if the field is empty or consists only of fillers.</returns>
    internal static bool IsAllFiller(ReadOnlySpan<char> field)
    {
        foreach (char character in field)
        {
            if (character != MrzConstants.FillerCharacter)
            {
                return false;
            }
        }

        return true;
    }
}
