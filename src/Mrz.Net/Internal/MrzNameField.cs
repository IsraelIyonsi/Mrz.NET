namespace Mrz.Internal;

/// <summary>
/// Parses the ICAO 9303 name field: a primary identifier (surname), the double-filler
/// separator, and a secondary identifier (given names), with single fillers standing in for
/// spaces and hyphens within each identifier.
/// </summary>
internal static class MrzNameField
{
    /// <summary>
    /// Splits a raw name field into its surname and given-names components.
    /// </summary>
    /// <param name="rawField">The name field exactly as it appears in the MRZ line.</param>
    /// <returns>
    /// The surname, the given names, and whether the field shows signs of truncation (no
    /// trailing filler padding, meaning the identifiers may have filled the field exactly and
    /// been cut off).
    /// </returns>
    internal static (string Surname, string GivenNames, bool IsTruncated) Parse(string rawField)
    {
        bool isTruncated = rawField.Length > 0 && rawField[^1] != MrzConstants.FillerCharacter;
        string trimmed = rawField.TrimEnd(MrzConstants.FillerCharacter);

        int separatorIndex = trimmed.IndexOf(MrzConstants.NameComponentSeparator, StringComparison.Ordinal);
        string primaryRaw = separatorIndex >= 0 ? trimmed[..separatorIndex] : trimmed;
        string secondaryRaw = separatorIndex >= 0
            ? trimmed[(separatorIndex + MrzConstants.NameComponentSeparator.Length)..]
            : string.Empty;

        return (NormalizeComponent(primaryRaw), NormalizeComponent(secondaryRaw), isTruncated);
    }

    private static string NormalizeComponent(string raw)
    {
        string[] words = raw.Split(MrzConstants.FillerCharacter, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', words);
    }
}
