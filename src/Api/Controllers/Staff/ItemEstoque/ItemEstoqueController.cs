using Api.Contracts.Validation;
using Api.Controllers.Staff.ItemEstoque.CreateItemEstoque;
using Api.Controllers.Staff.ItemEstoque.RegistrarEntradaEstoque;
using Api.Controllers.Staff.ItemEstoque.UpdateItemEstoque;
using Api.Presenters.ItemEstoque;
using Application.UseCases.Estoque.ItemEstoque.Commands.CreateItemEstoque;
using Application.UseCases.Estoque.ItemEstoque.Commands.DeleteItemEstoque;
using Application.UseCases.Estoque.ItemEstoque.Commands.RegistrarEntradaEstoque;
using Application.UseCases.Estoque.ItemEstoque.Commands.UpdateItemEstoque;
using Application.UseCases.Estoque.ItemEstoque.Queries.GetAllItensEstoque;
using Application.UseCases.Estoque.ItemEstoque.Queries.GetItemEstoqueById;
using BuildingBlocks.Http.Auth;
using Domain.Estoque.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Staff.ItemEstoque;

[ApiController]
[Route("ordem-servico/itens-estoque")]
[Authorize(Policy = AuthPolicies.FuncionarioJwt)]
public class ItemEstoqueController
(
    IMediator mediator,
    ItemEstoquePresenter presenter,
    IValidator<CreateItemEstoqueRequest> createValidator,
    IValidator<UpdateItemEstoqueRequest> updateValidator,
    IValidator<RegistrarEntradaEstoqueRequest> registrarEntradaValidator
) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateItemEstoqueRequest request)
    {
        var validationResult = createValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { validationResult.Errors });
        }

        var command = new CreateItemEstoqueCommand
        {
            Codigo = request.Codigo,
            Tipo = request.Tipo,
            Nome = request.Nome,
            UnidadeMedida = request.UnidadeMedida,
            PrecoVenda = request.PrecoVenda,
            Saldo = request.Saldo,
            SaldoReservado = request.SaldoReservado
        };

        var response = await mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, presenter.Present(response));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetItemEstoqueByIdQuery { Id = id };
        var response = await mediator.Send(query);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(presenter.Present(response));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllItemEstoqueQuery();
        var response = await mediator.Send(query);
        return Ok(presenter.Present(response));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateItemEstoqueRequest request)
    {
        var validationResult = updateValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { validationResult.Errors });
        }

        var command = new UpdateItemEstoqueCommand
        {
            Id = id,
            Codigo = request.Codigo,
            Tipo = (ItemTipo)request.Tipo,
            Nome = request.Nome,
            UnidadeMedida = (UnidadeMedida)request.UnidadeMedida,
            PrecoVenda = request.PrecoVenda,
        };

        var response = await mediator.Send(command);
        return Ok(presenter.Present(response));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteItemEstoqueCommand { Id = id };
        await mediator.Send(command);
        return NoContent();
    }

    [HttpPost("{id:int}/registrar-entrada")]
    public async Task<IActionResult> RegistrarEntrada(int id, [FromBody] RegistrarEntradaEstoqueRequest request)
    {
        var validationResult = registrarEntradaValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { validationResult.Errors });
        }

        var command = new RegistrarEntradaEstoqueCommand
        {
            IdItemEstoque = id,
            QuantidadeRecebida = request.Quantidade
        };

        var response = await mediator.Send(command);
        return Ok(presenter.Present(response));
    }
}
