using Mrz.Internal;
using Mrz.Tests.Fixtures;

namespace Mrz.Tests.Parsing;

public sealed class MrzSexParsingTests
{
    [Theory]
    [InlineData('M', MrzSex.Male)]
    [InlineData('F', MrzSex.Female)]
    [InlineData('<', MrzSex.Unspecified)]
    [InlineData('X', MrzSex.Unspecified)]
    public void Parse_SexMarker_MapsToExpectedEnumValue(char sexCharacter, MrzSex expectedSex)
    {
        string[] lines = (string[])IcaoSpecimens.Td3Eriksson.Lines.Clone();
        char[] line2 = lines[1].ToCharArray();
        line2[Td3Layout.SexOffset] = sexCharacter;
        lines[1] = new string(line2);

        MrzDocument document = MrzParser.Parse(lines);

        Assert.Equal(expectedSex, document.Sex);
        Assert.True(document.Validation.IsValid);
    }
}
