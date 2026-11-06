using Api.Auth;
using Api.Filters;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using Api.Contracts.Validation;
using Api.Controllers.Auth.Login;
using Api.Controllers.Cliente.CreateCliente;
using Api.Controllers.Cliente.UpdateCliente;
using Api.Controllers.ItemEstoque.CreateItemEstoque;
using Api.Controllers.ItemEstoque.RegistrarEntradaEstoque;
using Api.Controllers.ItemEstoque.UpdateItemEstoque;
using Api.Controllers.OrdemServico.AdicionarItemServico;
using Api.Controllers.OrdemServico.AprovarServicosParcialmente;
using Api.Controllers.OrdemServico.ConfirmarExecucao;
using Api.Controllers.OrdemServico.CriarOrdemServico;
using Api.Controllers.Servico.CreateServico;
using Api.Controllers.Servico.UpdateServico;
using Api.Controllers.Veiculo.CreateVeiculo;
using Api.Controllers.Veiculo.UpdateVeiculo;
using Api.Presenters.Auth;
using Api.Presenters.Cliente;
using Api.Presenters.ItemEstoque;
using Api.Presenters.OrdemServico;
using Api.Presenters.Servico;
using Api.Presenters.Veiculo;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.UseCases.Administrativo.Usuario.Commands.Login;
using Application.Abstractions.Gateways;
using Infrastructure.Database;
using Infrastructure.Database.Gateways;
using Infrastructure.Messaging;

namespace Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddCoreServices();
        services.AddUsuarioServices();
        services.AddClienteServices();
        services.AddVeiculoServices();
        services.AddServicoServices();
        services.AddItemEstoqueServices();
        services.AddOrdemServicoServices();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LoginCommand).Assembly));
        services.AddControllers(options =>
            {
                options.Filters.Add<ProblemDetailsExceptionFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        services.AddJwtAuthentication(configuration);
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.ClienteJwt, policy =>
            {
                policy.AddAuthenticationSchemes(AuthSchemes.Cliente);
                policy.RequireAuthenticatedUser();
                policy.RequireClaim(ClienteJwtClaims.Documento);
            });
        });
    }

    private static void AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<INotificacaoService, NotificacaoService>();
        services.AddSingleton<ISmtpService, SmtpService>();
    }

    private static void AddUsuarioServices(this IServiceCollection services)
    {
        services.AddSingleton<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IUsuarioGateway, UsuarioGateway>();
        services.AddScoped<AuthPresenter>();
    }

    private static void AddClienteServices(this IServiceCollection services)
    {
        services.AddScoped<IClienteGateway, ClienteGateway>();
        services.AddScoped<ClientePresenter>();
        services.AddSingleton<IValidator<CreateClienteRequest>, CreateClienteRequestValidator>();
        services.AddSingleton<IValidator<UpdateClienteRequest>, UpdateClienteRequestValidator>();
    }

    private static void AddVeiculoServices(this IServiceCollection services)
    {
        services.AddScoped<IVeiculoGateway, VeiculoGateway>();
        services.AddScoped<VeiculoPresenter>();
        services.AddSingleton<IValidator<CreateVeiculoRequest>, CreateVeiculoRequestValidator>();
        services.AddSingleton<IValidator<UpdateVeiculoRequest>, UpdateVeiculoRequestValidator>();
    }

    private static void AddServicoServices(this IServiceCollection services)
    {
        services.AddScoped<IServicoGateway, ServicoGateway>();
        services.AddScoped<ServicoPresenter>();
        services.AddSingleton<IValidator<CreateServicoRequest>, CreateServicoRequestValidator>();
        services.AddSingleton<IValidator<UpdateServicoRequest>, UpdateServicoRequestValidator>();
    }

    private static void AddItemEstoqueServices(this IServiceCollection services)
    {
        services.AddScoped<IItemEstoqueGateway, ItemEstoqueGateway>();
        services.AddScoped<ItemEstoquePresenter>();
        services.AddSingleton<IValidator<CreateItemEstoqueRequest>, CreateItemEstoqueRequestValidator>();
        services.AddSingleton<IValidator<UpdateItemEstoqueRequest>, UpdateItemEstoqueRequestValidator>();
        services.AddSingleton<IValidator<RegistrarEntradaEstoqueRequest>, RegistrarEntradaEstoqueRequestValidator>();
    }
    
    private static void AddOrdemServicoServices(this IServiceCollection services)
    {
        services.AddScoped<IOrdemServicoGateway, OrdemServicoGateway>();
        services.AddScoped<IItemServicoGateway, ItemServicoGateway>();
        services.AddSingleton<IDiagnosticoFinalizadoPublisher, NullDiagnosticoFinalizadoPublisher>();
        services.AddScoped<OrdemServicoPresenter>();
        services.AddSingleton<IValidator<CriarOrdemServicoRequest>, CriarOrdemServicoRequestValidator>();
        services.AddSingleton<IValidator<AdicionarItemServicoRequest>, AdicionarItemServicoRequestValidator>();
        services.AddSingleton<IValidator<AprovarServicosParcialmenteRequest>, AprovarServicosParcialmenteRequestValidator>();
        services.AddSingleton<IValidator<ConfirmarExecucaoRequest>, ConfirmarExecucaoRequestValidator>();
    }

    private static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtKey = configuration["JwtFuncionario:Key"] ?? "default-key";
        var jwtIssuer = configuration["JwtFuncionario:Issuer"] ?? "default-issuer";
        var jwtAudience = configuration["JwtFuncionario:Audience"] ?? "default-audience";

        var jwtClienteKey = configuration["JwtCliente:Key"] ?? "default-cliente-key";
        var jwtClienteIssuer = configuration["JwtCliente:Issuer"] ?? "default-cliente-issuer";
        var jwtClienteAudience = configuration["JwtCliente:Audience"] ?? "default-cliente-audience";

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = AuthSchemes.Funcionario;
                options.DefaultChallengeScheme = AuthSchemes.Funcionario;
            })
            .AddJwtBearer(AuthSchemes.Funcionario, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience
                };
            })
            .AddJwtBearer(AuthSchemes.Cliente, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtClienteKey)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = jwtClienteIssuer,
                    ValidAudience = jwtClienteAudience
                };
            });
    }
}
