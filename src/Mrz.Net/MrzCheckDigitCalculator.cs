using Mrz.Internal;

namespace Mrz;

/// <summary>
/// Computes and verifies ICAO 9303 check digits using the 7-3-1 weighted modulus-10 algorithm.
/// </summary>
/// <remarks>
/// Every character in the input is converted to a numeric value (digits keep their value, A-Z
/// map to 10-35, the filler character '&lt;' maps to 0), multiplied by a weight that cycles 7,
/// 3, 1 from the first character, summed, and reduced modulo 10. This is the single algorithm
/// ICAO 9303 uses for the document number, date of birth, date of expiry, personal number, and
/// composite check digits alike; only the input span differs per field.
/// </remarks>
public static class MrzCheckDigitCalculator
{
    /// <summary>
    /// Computes the ICAO 9303 check digit for <paramref name="field"/>.
    /// </summary>
    /// <param name="field">The MRZ text the check digit protects.</param>
    /// <returns>The computed check digit, a character '0'-'9'.</returns>
    /// <exception cref="MrzFormatException">
    /// <paramref name="field"/> contains a character outside the MRZ character set.
    /// </exception>
    public static char Compute(ReadOnlySpan<char> field)
    {
        int weightedSum = 0;

        for (int index = 0; index < field.Length; index++)
        {
            int value = MrzCharacterValue.GetValue(field[index]);
            int weight = MrzConstants.CheckDigitWeights[index % MrzConstants.CheckDigitWeights.Length];
            weightedSum += value * weight;
        }

        int checkDigitValue = weightedSum % MrzConstants.CheckDigitModulus;
        return (char)(MrzConstants.DigitZero + checkDigitValue);
    }

    /// <summary>
    /// Verifies that <paramref name="checkDigit"/> is the correct ICAO 9303 check digit for
    /// <paramref name="field"/>.
    /// </summary>
    /// <param name="field">The MRZ text the check digit protects.</param>
    /// <param name="checkDigit">The check digit as printed in the machine-readable zone.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="checkDigit"/> matches the computed check digit,
    /// or if <paramref name="checkDigit"/> is the filler character and <paramref name="field"/>
    /// is entirely filler (ICAO 9303 permits the filler character in place of a check digit for
    /// an unused optional field); otherwise <see langword="false"/>.
    /// </returns>
    /// <exception cref="MrzFormatException">
    /// <paramref name="field"/> contains a character outside the MRZ character set.
    /// </exception>
    public static bool Verify(ReadOnlySpan<char> field, char checkDigit)
    {
        if (checkDigit == MrzConstants.FillerCharacter && MrzCharacterValue.IsAllFiller(field))
        {
            return true;
        }

        return Compute(field) == checkDigit;
    }
}
