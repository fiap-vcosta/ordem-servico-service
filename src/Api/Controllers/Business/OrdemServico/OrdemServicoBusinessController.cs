using System.Security.Claims;
using Api.Presenters.OrdemServico;
using Application.UseCases.Commands.AprovarOrdemServicoPorToken;
using Application.UseCases.Commands.RejeitarOrdemServicoPorToken;
using BuildingBlocks.Http.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Business.OrdemServico;

[ApiController]
[Authorize(Policy = AuthPolicies.ClienteJwt)]
[Route("ordem-servico")]
public class OrdemServicoBusinessController(IMediator mediator, OrdemServicoPresenter presenter) : ControllerBase
{
    private static readonly string[] TokenObrigatorioErrors = ["Token é obrigatório."];

    [HttpPost("aprovar")]
    public async Task<IActionResult> Aprovar([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new { Errors = TokenObrigatorioErrors });
        }

        var documentoCliente = User.FindFirstValue(ClienteJwtClaims.Documento);
        if (string.IsNullOrWhiteSpace(documentoCliente))
        {
            return Unauthorized();
        }

        var response = await mediator.Send(new AprovarOrdemServicoPorTokenCommand
        {
            TokenAprovacao = token,
            DocumentoCliente = documentoCliente
        });
        return Ok(presenter.Present(response));
    }

    [HttpPost("rejeitar")]
    public async Task<IActionResult> Rejeitar([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new { Errors = TokenObrigatorioErrors });
        }

        var documentoCliente = User.FindFirstValue(ClienteJwtClaims.Documento);
        if (string.IsNullOrWhiteSpace(documentoCliente))
        {
            return Unauthorized();
        }

        var response = await mediator.Send(new RejeitarOrdemServicoPorTokenCommand
        {
            TokenAprovacao = token,
            DocumentoCliente = documentoCliente
        });
        return Ok(presenter.Present(response));
    }
}
