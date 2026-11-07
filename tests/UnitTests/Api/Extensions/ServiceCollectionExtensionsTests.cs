using Api.Contracts.Validation;
using Api.Controllers.Staff.OrdemServico.CriarOrdemServico;
using Api.Extensions;
using Application.Abstractions.Gateways;
using Application.Abstractions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests.Api.Extensions;

public class ServiceCollectionExtensionsTests
{
    private const string TestPublicKeyPem =
        """
        -----BEGIN PUBLIC KEY-----
        MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAnplHBIKLXAyikvjQ30TQ
        wP+Pq6SLaJJjLlWGk6Q+8JuYYjU6xocJqoGebmhnRk4ZIZ+peY8O9wqABzWD3Nl5
        xf+kEMKwvvDKX3r2yanQiAzo6qPWDWINC0/QAcc7BCodhqWxQjnuNChBRc7SGVCH
        nJX1GmJqQVlj554aEmmP9+luog5johHY38s0EudmxfC22fPywDjtOYp6+RyzVsf1
        cpLTytVfjOLRIokJBsVxGsS3xkRaMYoiBA1lQOBqQajHdw5O7uVyxicxdeKdVwFd
        7rDQBFZl12SgJzZg9lrQLiN31vqfyQooClbMyme2w8nePLDgqe02kKf4Pir+u/qz
        gQIDAQAB
        -----END PUBLIC KEY-----
        """;

    [Fact]
    public void AddApiServices_RegistersCoreDependencies()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=postgres;Password=postgres;Maximum Pool Size=15",
                ["Jwt:Funcionario:Issuer"] = "test-issuer",
                ["Jwt:Funcionario:Audience"] = "test-audience",
                ["Jwt:Funcionario:PublicKeyPem"] = TestPublicKeyPem,
                ["Jwt:Funcionario:RequireHttpsMetadata"] = "false",
                ["Jwt:Cliente:Issuer"] = "test-issuer",
                ["Jwt:Cliente:Audience"] = "test-audience",
                ["Jwt:Cliente:PublicKeyPem"] = TestPublicKeyPem,
                ["Jwt:Cliente:RequireHttpsMetadata"] = "false",
                ["Administrativo:BaseUrl"] = "http://localhost:8080",
                ["ServiceAuth:Key"] = "integration-service-auth-key"
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddApiServices(configuration);
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var scoped = scope.ServiceProvider;

        // Assert
        Assert.NotNull(provider.GetService<INotificacaoService>());
        Assert.NotNull(provider.GetService<ISmtpService>());
        Assert.NotNull(provider.GetService<IValidator<CriarOrdemServicoRequest>>());
        Assert.NotNull(scoped.GetRequiredService<IAdministrativoCatalogGateway>());
        Assert.NotNull(scoped.GetRequiredService<IItemEstoqueGateway>());
        Assert.NotNull(scoped.GetRequiredService<IOrdemServicoGateway>());
        Assert.NotNull(scoped.GetRequiredService<IItemServicoGateway>());
    }
}
