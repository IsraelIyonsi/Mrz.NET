namespace Mrz;

/// <summary>
/// The ICAO Doc 9303 machine-readable-zone size and field layout that a document uses.
/// </summary>
public enum MrzDocumentType
{
    /// <summary>
    /// TD1: three lines of 30 characters each, used by ID cards (ICAO Doc 9303 Part 5).
    /// </summary>
    Td1,

    /// <summary>
    /// TD2: two lines of 36 characters each, used by ID cards and visas (ICAO Doc 9303 Part 6).
    /// </summary>
    Td2,

    /// <summary>
    /// TD3: two lines of 44 characters each, used by passports (ICAO Doc 9303 Part 4).
    /// </summary>
    Td3,

    /// <summary>
    /// MRV-A: a machine-readable visa in the two-lines-of-44 geometry (the same size as TD3),
    /// distinguished by a document code beginning with "V" (ICAO Doc 9303 Part 7). Unlike a
    /// passport, a visa carries no overall composite check digit; the trailing positions on
    /// line 2 are optional data.
    /// </summary>
    MrvA,

    /// <summary>
    /// MRV-B: a machine-readable visa in the two-lines-of-36 geometry (the same size as TD2),
    /// distinguished by a document code beginning with "V" (ICAO Doc 9303 Part 7). Unlike a
    /// TD2 ID card, a visa carries no overall composite check digit; the trailing positions on
    /// line 2 are optional data.
    /// </summary>
    MrvB,
}
