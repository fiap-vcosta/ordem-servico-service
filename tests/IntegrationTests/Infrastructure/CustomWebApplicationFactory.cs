using Application.Abstractions.Gateways;
using IntegrationTests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;

    public CustomWebApplicationFactory()
    {
        EnsureDockerHost();
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("ordem_servico_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            _postgres.GetConnectionString() + ";Maximum Pool Size=15");
        Environment.SetEnvironmentVariable("Jwt__Funcionario__Issuer", TestJwt.Issuer);
        Environment.SetEnvironmentVariable("Jwt__Funcionario__Audience", TestJwt.FuncionarioAudience);
        Environment.SetEnvironmentVariable("Jwt__Funcionario__PublicKeyPem", TestJwt.PublicKeyPem);
        Environment.SetEnvironmentVariable("Jwt__Funcionario__RequireHttpsMetadata", "false");
        Environment.SetEnvironmentVariable("Jwt__Cliente__Issuer", TestJwt.Issuer);
        Environment.SetEnvironmentVariable("Jwt__Cliente__Audience", TestJwt.ClienteAudience);
        Environment.SetEnvironmentVariable("Jwt__Cliente__PublicKeyPem", TestJwt.PublicKeyPem);
        Environment.SetEnvironmentVariable("Jwt__Cliente__RequireHttpsMetadata", "false");
        Environment.SetEnvironmentVariable("ServiceAuth__Key", "integration-service-auth-key");
        Environment.SetEnvironmentVariable("Administrativo__BaseUrl", "http://localhost:9");
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        var connectionString = _postgres.GetConnectionString() + ";Maximum Pool Size=15";
        var publicPem = TestJwt.PublicKeyPem;

        builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);
        builder.UseSetting("Jwt:Funcionario:Issuer", TestJwt.Issuer);
        builder.UseSetting("Jwt:Funcionario:Audience", TestJwt.FuncionarioAudience);
        builder.UseSetting("Jwt:Funcionario:PublicKeyPem", publicPem);
        builder.UseSetting("Jwt:Funcionario:RequireHttpsMetadata", "false");
        builder.UseSetting("Jwt:Cliente:Issuer", TestJwt.Issuer);
        builder.UseSetting("Jwt:Cliente:Audience", TestJwt.ClienteAudience);
        builder.UseSetting("Jwt:Cliente:PublicKeyPem", publicPem);
        builder.UseSetting("Jwt:Cliente:RequireHttpsMetadata", "false");
        builder.UseSetting("ServiceAuth:Key", "integration-service-auth-key");
        builder.UseSetting("Administrativo:BaseUrl", "http://localhost:9");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["Jwt:Funcionario:Issuer"] = TestJwt.Issuer,
                ["Jwt:Funcionario:Audience"] = TestJwt.FuncionarioAudience,
                ["Jwt:Funcionario:PublicKeyPem"] = publicPem,
                ["Jwt:Funcionario:RequireHttpsMetadata"] = "false",
                ["Jwt:Cliente:Issuer"] = TestJwt.Issuer,
                ["Jwt:Cliente:Audience"] = TestJwt.ClienteAudience,
                ["Jwt:Cliente:PublicKeyPem"] = publicPem,
                ["Jwt:Cliente:RequireHttpsMetadata"] = "false",
                ["ServiceAuth:Key"] = "integration-service-auth-key",
                ["Administrativo:BaseUrl"] = "http://localhost:9"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IAdministrativoCatalogGateway>();
            services.AddSingleton<IAdministrativoCatalogGateway, StubAdministrativoCatalogGateway>();
        });
    }

    private static void EnsureDockerHost()
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DOCKER_HOST")))
        {
            return;
        }

        if (File.Exists("/var/run/docker.sock"))
        {
            Environment.SetEnvironmentVariable("DOCKER_HOST", "unix:///var/run/docker.sock");
        }
    }
}

[CollectionDefinition(nameof(IntegrationFixture))]
public class IntegrationFixture : ICollectionFixture<CustomWebApplicationFactory>;
