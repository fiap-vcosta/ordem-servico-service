using Infrastructure.Http;

namespace UnitTests.Infrastructure.Http;

public class HttpOptionsTests
{
    [Fact]
    public void AdministrativoOptions_HoldsBaseUrl()
    {
        // Arrange & Act
        var options = new AdministrativoOptions { BaseUrl = "http://administrativo" };

        // Assert
        Assert.Equal("Administrativo", AdministrativoOptions.SectionName);
        Assert.Equal("http://administrativo", options.BaseUrl);
    }

    [Fact]
    public void EstoqueOptions_HoldsBaseUrl()
    {
        // Arrange & Act
        var options = new EstoqueOptions { BaseUrl = "http://estoque" };

        // Assert
        Assert.Equal("Estoque", EstoqueOptions.SectionName);
        Assert.Equal("http://estoque", options.BaseUrl);
    }
}
