using Application.Abstractions.Gateways;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Infrastructure.Database;
using Infrastructure.Database.Gateways;
using Infrastructure.Http;
using Infrastructure.Messaging;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IOrdemServicoGateway, OrdemServicoGateway>();
        services.AddScoped<IItemServicoGateway, ItemServicoGateway>();
        services.AddScoped<IItemEstoqueGateway, ItemEstoqueGateway>();
        services.AddSingleton<IDiagnosticoFinalizadoPublisher, NullDiagnosticoFinalizadoPublisher>();
        services.AddSingleton<INotificacaoService, NotificacaoService>();
        services.AddSingleton<ISmtpService, SmtpService>();

        services.Configure<AdministrativoOptions>(configuration.GetSection(AdministrativoOptions.SectionName));

        var administrativoBaseUrl = configuration[$"{AdministrativoOptions.SectionName}:BaseUrl"]
            ?? throw new InvalidOperationException("Administrativo:BaseUrl é obrigatório.");
        var serviceAuthKey = configuration["ServiceAuth:Key"]
            ?? throw new InvalidOperationException("ServiceAuth:Key é obrigatório.");

        services.AddHttpClient<IAdministrativoCatalogGateway, AdministrativoCatalogGateway>(client =>
        {
            client.BaseAddress = new Uri(administrativoBaseUrl.TrimEnd('/') + "/");
            client.DefaultRequestHeaders.Add("X-Service-Key", serviceAuthKey);
        });

        return services;
    }
}
