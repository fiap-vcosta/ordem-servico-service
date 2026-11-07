using Application.Abstractions.Gateways;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Messaging;
using BuildingBlocks.Events;
using BuildingBlocks.Messaging;
using BuildingBlocks.Tracing;
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
    public const string SubscriptionOrcamentoGerado = "ordem-servico-orcamento-gerado";
    public const string SubscriptionOrcamentoFalhou = "ordem-servico-orcamento-falhou";
    public const string SubscriptionEstoqueReservado = "ordem-servico-estoque-reservado";
    public const string SubscriptionEstoqueEmFalta = "ordem-servico-estoque-em-falta";
    public const string SubscriptionCobrancaGerada = "ordem-servico-cobranca-gerada";
    public const string SubscriptionPagamentoConfirmado = "ordem-servico-pagamento-confirmado";
    public const string SubscriptionPagamentoRecusado = "ordem-servico-pagamento-recusado";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IOrdemServicoGateway, OrdemServicoGateway>();
        services.AddScoped<IItemServicoGateway, ItemServicoGateway>();
        services.AddScoped<IEventoProcessadoGateway, EventoProcessadoGateway>();
        services.AddSingleton<IOrdemServicoIntegrationPublisher, OrdemServicoIntegrationPublisher>();
        services.AddSingleton<INotificacaoService, NotificacaoService>();
        services.AddSingleton<ISmtpService, SmtpService>();

        services.Configure<AdministrativoOptions>(configuration.GetSection(AdministrativoOptions.SectionName));
        services.Configure<EstoqueOptions>(configuration.GetSection(EstoqueOptions.SectionName));

        var administrativoBaseUrl = configuration[$"{AdministrativoOptions.SectionName}:BaseUrl"]
            ?? throw new InvalidOperationException("Administrativo:BaseUrl é obrigatório.");
        var estoqueBaseUrl = configuration[$"{EstoqueOptions.SectionName}:BaseUrl"]
            ?? throw new InvalidOperationException("Estoque:BaseUrl é obrigatório.");
        var serviceAuthKey = configuration["ServiceAuth:Key"]
            ?? throw new InvalidOperationException("ServiceAuth:Key é obrigatório.");

        services.AddHttpClient<IAdministrativoCatalogGateway, AdministrativoCatalogGateway>(client =>
        {
            client.BaseAddress = new Uri(administrativoBaseUrl.TrimEnd('/') + "/");
            client.DefaultRequestHeaders.Add("X-Service-Key", serviceAuthKey);
        });

        services.AddHttpClient<IEstoqueCatalogGateway, EstoqueCatalogGateway>(client =>
        {
            client.BaseAddress = new Uri(estoqueBaseUrl.TrimEnd('/') + "/");
            client.DefaultRequestHeaders.Add("X-Service-Key", serviceAuthKey);
        });

        var useInMemoryPubSub = configuration.GetValue("PubSub:UseInMemory", false);
        if (useInMemoryPubSub)
        {
            services.AddSingleton<InMemoryPubSubTransport>();
            services.AddSingleton<IPubSubTransport>(sp => sp.GetRequiredService<InMemoryPubSubTransport>());
            services.AddSingleton<IMessageTracePropagator, DatadogMessageTracePropagator>();
            services.AddSingleton<IEventPublisher, EventPublisher>();
            services.AddOptions<PubSubOptions>();
        }
        else
        {
            services.Configure<PubSubOptions>(configuration.GetSection(PubSubOptions.SectionName));
            services.AddBuildingBlocksMessaging();
        }

        if (configuration.GetValue("PubSub:EnableWorkers", true))
        {
            services.AddEventSubscriptionWorker<OrcamentoGeradoData, OrcamentoGeradoConsumer>(
                configuration["PubSub:Subscriptions:OrcamentoGerado"] ?? SubscriptionOrcamentoGerado);
            services.AddEventSubscriptionWorker<OrcamentoFalhouData, OrcamentoFalhouConsumer>(
                configuration["PubSub:Subscriptions:OrcamentoFalhou"] ?? SubscriptionOrcamentoFalhou);
            services.AddEventSubscriptionWorker<EstoqueReservadoData, EstoqueReservadoConsumer>(
                configuration["PubSub:Subscriptions:EstoqueReservado"] ?? SubscriptionEstoqueReservado);
            services.AddEventSubscriptionWorker<EstoqueEmFaltaData, EstoqueEmFaltaConsumer>(
                configuration["PubSub:Subscriptions:EstoqueEmFalta"] ?? SubscriptionEstoqueEmFalta);
            services.AddEventSubscriptionWorker<CobrancaGeradaData, CobrancaGeradaConsumer>(
                configuration["PubSub:Subscriptions:CobrancaGerada"] ?? SubscriptionCobrancaGerada);
            services.AddEventSubscriptionWorker<PagamentoConfirmadoData, PagamentoConfirmadoConsumer>(
                configuration["PubSub:Subscriptions:PagamentoConfirmado"] ?? SubscriptionPagamentoConfirmado);
            services.AddEventSubscriptionWorker<PagamentoRecusadoData, PagamentoRecusadoConsumer>(
                configuration["PubSub:Subscriptions:PagamentoRecusado"] ?? SubscriptionPagamentoRecusado);
        }

        return services;
    }
}
