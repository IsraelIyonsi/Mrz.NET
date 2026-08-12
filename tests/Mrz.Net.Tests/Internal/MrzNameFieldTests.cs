using Mrz.Internal;

namespace Mrz.Tests.Internal;

public sealed class MrzNameFieldTests
{
    public static IEnumerable<object[]> ParseCases()
    {
        // Surname and given names separated by the double-filler separator, trailing padded.
        yield return new object[] { "ERIKSSON<<ANNA<MARIA<<<<<<<<<<<<<<<<<<<", "ERIKSSON", "ANNA MARIA", false };

        // No secondary identifier at all.
        yield return new object[] { "OKONKWO<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<", "OKONKWO", "", false };

        // Multi-word surname using single fillers for spaces within the primary identifier.
        yield return new object[] { "VAN<DER<BERG<<ANNA<<<<<<<<<<<<<<<<<<<<<<", "VAN DER BERG", "ANNA", false };

        // Multiple given names.
        yield return new object[] { "SMITH<<JOHN<PAUL<GEORGE<<<<<<<<<<<<<<<<<", "SMITH", "JOHN PAUL GEORGE", false };

        // Field filled exactly to capacity: no trailing filler, so truncation is flagged.
        yield return new object[] { "PATTERSON<<ELIZABETH<CHARLOTTE", "PATTERSON", "ELIZABETH CHARLOTTE", true };

        // Entirely filler: no identifiers at all.
        yield return new object[] { "<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<", "", "", false };

        // Single character surname, no padding at all (still truncated by the "no trailing filler" rule).
        yield return new object[] { "X", "X", "", true };
    }

    [Theory]
    [MemberData(nameof(ParseCases))]
    public void Parse_ReturnsExpectedComponents(string rawField, string expectedSurname, string expectedGivenNames, bool expectedTruncated)
    {
        (string surname, string givenNames, bool isTruncated) = MrzNameField.Parse(rawField);

        Assert.Equal(expectedSurname, surname);
        Assert.Equal(expectedGivenNames, givenNames);
        Assert.Equal(expectedTruncated, isTruncated);
    }
}
