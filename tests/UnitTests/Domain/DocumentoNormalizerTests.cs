using Domain;

namespace UnitTests.Domain;

public class DocumentoNormalizerTests
{
    [Theory]
    [InlineData("864.215.370-90", "86421537090")]
    [InlineData("11.222.333/0001-81", "11222333000181")]
    [InlineData(" 86421537090 ", "86421537090")]
    public void Normalize_StripsFormatting(string input, string expected)
    {
        Assert.Equal(expected, DocumentoNormalizer.Normalize(input));
    }
}
