using Api.Contracts.Validation;
using Api.Controllers.OrdemServico.AdicionarItemServico;
using Api.Controllers.OrdemServico.AprovarServicosParcialmente;
using Api.Controllers.OrdemServico.ConfirmarExecucao;
using Api.Controllers.OrdemServico.CriarOrdemServico;
using Api.Presenters.OrdemServico;
using Application.UseCases.OrdemServico.Commands.AdicionarItemOrdemServico;
using Application.UseCases.OrdemServico.Commands.AprovarOrdemServico;
using Application.UseCases.OrdemServico.Commands.AprovarServicosParcialmente;
using Application.UseCases.OrdemServico.Commands.ConfirmarExecucaoOrdemServico;
using Application.UseCases.OrdemServico.Commands.CriarOrdemServico;
using Application.UseCases.OrdemServico.Commands.DescartarOrdemServico;
using Application.UseCases.OrdemServico.Commands.FinalizarDiagnostico;
using Application.UseCases.OrdemServico.Commands.RejeitarOrdemServico;
using Application.UseCases.OrdemServico.Queries.GetOrdemServicoById;
using Application.UseCases.OrdemServico.Queries.GetTempoMedioAllServicos;
using Application.UseCases.OrdemServico.Queries.ListarOrdensServico;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.OrdemServico;

[ApiController]
[Route("api/ordens-servico")]
[Authorize(Roles = "Admin")]
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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
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

    [HttpPost("{id:int}/descartar")]
    public async Task<IActionResult> DescartarOrdemServico(int id)
    {
        var command = new DescartarOrdemServicoCommand() { IdOrdemServico = id };
        var response = await mediator.Send(command);
        return Ok(presenter.Present(response));
    }

    [HttpPost("{id:int}/adicionar-servico")]
    public async Task<IActionResult> AdicionarItemServico(int id, [FromBody] AdicionarItemServicoRequest request)
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

    [HttpPost("{id:int}/finalizar-diagnostico")]
    public async Task<IActionResult> FinalizarDiagnostico(int id)
    {
        var command = new FinalizarDiagnosticoCommand() { IdOrdemServico = id };
        var response = await mediator.Send(command);
        return Ok(presenter.Present(response));
    }

    [HttpPost("{id:int}/rejeitar")]
    public async Task<IActionResult> RejeitarOrdemServico(int id)
    {
        var command = new RejeitarOrdemServicoCommand() { IdOrdemServico = id };
        var response = await mediator.Send(command);
        return Ok(presenter.Present(response));
    }

    [HttpPost("{id:int}/aprovar")]
    public async Task<IActionResult> AprovarOrdemServico(int id)
    {
        var command = new AprovarOrdemServicoCommand() { IdOrdemServico = id };
        var response = await mediator.Send(command);
        return Ok(presenter.Present(response));
    }

    [HttpPost("{id:int}/aprovar-parcialmente")]
    public async Task<IActionResult> AprovarServicosParcialmente(int id, [FromBody] AprovarServicosParcialmenteRequest request)
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

    [HttpPost("{id:int}/confirmar-execucao")]
    public async Task<IActionResult> ConfirmarExecucao(int id, [FromBody] ConfirmarExecucaoRequest request)
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
