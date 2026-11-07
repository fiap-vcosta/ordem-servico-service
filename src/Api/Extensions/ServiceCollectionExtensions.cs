using System.Text.Json.Serialization;
using Api.Contracts.Validation;
using Api.Controllers.Staff.OrdemServico.AdicionarItemServico;
using Api.Controllers.Staff.OrdemServico.AprovarServicosParcialmente;
using Api.Controllers.Staff.OrdemServico.ConfirmarExecucao;
using Api.Controllers.Staff.OrdemServico.CriarOrdemServico;
using Api.Presenters.OrdemServico;
using Application.UseCases.Commands.CriarOrdemServico;
using BuildingBlocks.Http.Auth;
using BuildingBlocks.Http.ProblemDetails;
using Domain.Exceptions;
using Infrastructure.DependencyInjection;

namespace Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddScoped<OrdemServicoPresenter>();

        services.AddSingleton<IValidator<CriarOrdemServicoRequest>, CriarOrdemServicoRequestValidator>();
        services.AddSingleton<IValidator<AdicionarItemServicoRequest>, AdicionarItemServicoRequestValidator>();
        services.AddSingleton<IValidator<AprovarServicosParcialmenteRequest>, AprovarServicosParcialmenteRequestValidator>();
        services.AddSingleton<IValidator<ConfirmarExecucaoRequest>, ConfirmarExecucaoRequestValidator>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CriarOrdemServicoCommand).Assembly));

        services.AddBuildingBlocksProblemDetails(options =>
        {
            options.Map<DomainNotFoundException>(StatusCodes.Status404NotFound, "Not Found");
            options.Map<KeyNotFoundException>(StatusCodes.Status404NotFound, "Not Found");
            options.Map<BusinessRuleException>(StatusCodes.Status400BadRequest, "Business Rule Violation");
            options.Map<ArgumentException>(StatusCodes.Status400BadRequest, "Bad Request");
            options.Map<UnauthorizedAccessException>(StatusCodes.Status401Unauthorized, "Unauthorized");
        });

        services.AddControllers(options =>
            {
                options.Filters.Add<ProblemDetailsExceptionFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddBuildingBlocksJwtAuthentication(configuration);
    }
}
