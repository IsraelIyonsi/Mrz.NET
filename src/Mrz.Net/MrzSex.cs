namespace Mrz;

/// <summary>
/// The sex marker recorded in the machine-readable zone.
/// </summary>
public enum MrzSex
{
    /// <summary>
    /// The document records the holder's sex as male ('M').
    /// </summary>
    Male,

    /// <summary>
    /// The document records the holder's sex as female ('F').
    /// </summary>
    Female,

    /// <summary>
    /// The document leaves the sex marker unspecified, encoded as the filler character ('&lt;'),
    /// or uses the nonconformant 'X' marker some real-world issuers print for the same meaning.
    /// </summary>
    Unspecified,
}
