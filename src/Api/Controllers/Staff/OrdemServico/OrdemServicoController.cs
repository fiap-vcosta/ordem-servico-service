using Api.Contracts.Validation;
using Api.Controllers.Staff.OrdemServico.AdicionarItemServico;
using Api.Controllers.Staff.OrdemServico.AprovarServicosParcialmente;
using Api.Controllers.Staff.OrdemServico.ConfirmarExecucao;
using Api.Controllers.Staff.OrdemServico.CriarOrdemServico;
using Api.Presenters.OrdemServico;
using Application.UseCases.Commands.AdicionarItemOrdemServico;
using Application.UseCases.Commands.AprovarOrdemServico;
using Application.UseCases.Commands.AprovarServicosParcialmente;
using Application.UseCases.Commands.ConfirmarExecucaoOrdemServico;
using Application.UseCases.Commands.CriarOrdemServico;
using Application.UseCases.Commands.DescartarOrdemServico;
using Application.UseCases.Commands.FinalizarDiagnostico;
using Application.UseCases.Commands.RejeitarOrdemServico;
using Application.UseCases.Queries.GetOrdemServicoById;
using Application.UseCases.Queries.GetTempoMedioAllServicos;
using Application.UseCases.Queries.ListarOrdensServico;
using BuildingBlocks.Http.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Staff.OrdemServico;

[ApiController]
[Route("ordem-servico")]
[Authorize(Policy = AuthPolicies.FuncionarioJwt)]
public class OrdemServicoController(
    IMediator mediator,
    OrdemServicoPresenter presenter,
    IValidator<CriarOrdemServicoRequest> createOrdemServicoRequestValidator,
    IValidator<AdicionarItemServicoRequest> adicionarItemServicoRequestValidator,
    IValidator<AprovarServicosParcialmenteRequest> aprovarServicosParcialmenteRequestValidator,
    IValidator<ConfirmarExecucaoRequest> confirmarExecucaoRequestValidator
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var response = await mediator.Send(new ListarOrdensServicoQuery());
        return Ok(presenter.Present(response));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetOrdemServicoByIdQuery() { Id = id };
        var response = await mediator.Send(query);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(presenter.Present(response));
    }

    [HttpPost]
    public async Task<IActionResult> CriarOrdemServico([FromBody] CriarOrdemServicoRequest request)
    {
        var validationResult = createOrdemServicoRequestValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { validationResult.Errors });
        }

        var command = new CriarOrdemServicoCommand
        {
            IdVeiculo = request.IdVeiculo,
            Servicos = request.Servicos.Select(servico => new CriarOrdemServicoCommand.Servico
            {
                IdServico = servico.IdServico,
                ValorCobrado = servico.ValorCobrado,
                ItensNecessarios = servico.ItensNecessarios.Select(item =>
                    new CriarOrdemServicoCommand.ItemNecessario
                    {
                        IdItemEstoque = item.IdItemEstoque,
                        Quantidade = item.Quantidade
                    }).ToList()
            }).ToList()
        };

        var response = await mediator.Send(command);
        return Created(nameof(GetById), presenter.Present(response));
    }

    [HttpPost("{id:guid}/descartar")]
    public async Task<IActionResult> DescartarOrdemServico(Guid id)
    {
        var command = new DescartarOrdemServicoCommand() { IdOrdemServico = id };
        var response = await mediator.Send(command);
        return Ok(presenter.Present(response));
    }

    [HttpPost("{id:guid}/adicionar-servico")]
    public async Task<IActionResult> AdicionarItemServico(Guid id, [FromBody] AdicionarItemServicoRequest request)
    {
        var validationResult = adicionarItemServicoRequestValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { validationResult.Errors });
        }

        var command = new AdicionarItemOrdemServicoCommand
        {
            IdOrdemServico = id,
            IdServico = request.IdServico,
            ValorCobrado = request.ValorCobrado,
            ItensNecessarios = request.ItensNecessarios.Select(item =>
                new AdicionarItemOrdemServicoCommand.ItemNecessario
                {
                    IdItemEstoque = item.IdItemEstoque,
                    Quantidade = item.Quantidade
                }).ToList()
        };

        var response = await mediator.Send(command);
        return Ok(presenter.Present(response));
    }

    [HttpPost("{id:guid}/finalizar-diagnostico")]
    public async Task<IActionResult> FinalizarDiagnostico(Guid id)
    {
        var command = new FinalizarDiagnosticoCommand() { IdOrdemServico = id };
        var response = await mediator.Send(command);
        return Ok(presenter.Present(response));
    }

    [HttpPost("{id:guid}/rejeitar")]
    public async Task<IActionResult> RejeitarOrdemServico(Guid id)
    {
        var command = new RejeitarOrdemServicoCommand() { IdOrdemServico = id };
        var response = await mediator.Send(command);
        return Ok(presenter.Present(response));
    }

    [HttpPost("{id:guid}/aprovar")]
    public async Task<IActionResult> AprovarOrdemServico(Guid id)
    {
        var command = new AprovarOrdemServicoCommand() { IdOrdemServico = id };
        var response = await mediator.Send(command);
        return Ok(presenter.Present(response));
    }

    [HttpPost("{id:guid}/aprovar-parcialmente")]
    public async Task<IActionResult> AprovarServicosParcialmente(Guid id, [FromBody] AprovarServicosParcialmenteRequest request)
    {
        var validationResult = aprovarServicosParcialmenteRequestValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { validationResult.Errors });
        }

        var command = new AprovarServicosParcialmenteCommand()
        {
            IdOrdemServico = id,
            IdServicosAprovados = request.IdsServicosAprovados
        };

        var response = await mediator.Send(command);
        return Ok(presenter.Present(response));
    }

    [HttpPost("{id:guid}/confirmar-execucao")]
    public async Task<IActionResult> ConfirmarExecucao(Guid id, [FromBody] ConfirmarExecucaoRequest request)
    {
        var validationResult = confirmarExecucaoRequestValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { validationResult.Errors });
        }

        var command = new ConfirmarExecucaoOrdemServicoCommand()
        {
            IdOrdemServico = id,
            ServicosExecutados = request.ServicosExecutados
        };

        var response = await mediator.Send(command);
        return Ok(presenter.Present(response));
    }

    [HttpGet("tempo-medio-execucao")]
    public async Task<IActionResult> GetTempoMedioExecucao()
    {
        var query = new GetTempoMedioExecucaoAllServicosQuery();
        var response = await mediator.Send(query);
        return Ok(presenter.Present(response));
    }
}
