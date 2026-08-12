namespace Mrz;

/// <summary>
/// The exception thrown when machine-readable-zone text does not conform to the structural
/// requirements of ICAO Doc 9303 (wrong number of lines, wrong line length, or a character
/// outside the permitted MRZ character set).
/// </summary>
/// <remarks>
/// A failed check digit does not throw this exception. Check-digit outcomes are data-quality
/// results, exposed through <see cref="MrzValidationResult"/> on a successfully parsed
/// <see cref="MrzDocument"/>. This exception is reserved for input that cannot be laid out
/// into MRZ fields at all.
/// </remarks>
public sealed class MrzFormatException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MrzFormatException"/> class with a message
    /// describing the structural problem.
    /// </summary>
    /// <param name="message">A human-readable description of the structural problem.</param>
    public MrzFormatException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MrzFormatException"/> class with a message
    /// and the exception that caused it.
    /// </summary>
    /// <param name="message">A human-readable description of the structural problem.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public MrzFormatException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
