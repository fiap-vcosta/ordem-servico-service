using System.Net.Http.Headers;
using System.Security.Claims;
using BuildingBlocks.Http.Auth;
using IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

[Collection(nameof(IntegrationFixture))]
public class ClienteJwtSchemeTests
{
    private readonly CustomWebApplicationFactory _factory;

    public ClienteJwtSchemeTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ClienteScheme_AcceptsTokenSignedWithClienteKey()
    {
        // Arrange
        const string documento = "11144477735";
        var token = TestJwt.CreateClienteToken(documento);
        using var scope = _factory.Services.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
        {
            RequestServices = scope.ServiceProvider
        };
        httpContext.Request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token).ToString();

        // Act
        var result = await auth.AuthenticateAsync(httpContext, AuthSchemes.Cliente);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(documento, result.Principal?.FindFirstValue(ClienteJwtClaims.Documento));
    }

    [Fact]
    public async Task ClienteScheme_RejectsFuncionarioToken()
    {
        // Arrange
        var funcionarioToken = TestJwt.CreateStaffToken();
        using var scope = _factory.Services.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
        {
            RequestServices = scope.ServiceProvider
        };
        httpContext.Request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", funcionarioToken).ToString();

        // Act
        var result = await auth.AuthenticateAsync(httpContext, AuthSchemes.Cliente);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void FuncionarioToken_IsIssuedForStaffAuth()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        AuthHelper.AuthenticateAsStaff(client);

        // Assert
        Assert.NotNull(client.DefaultRequestHeaders.Authorization);
    }
}
