using System.Security.Claims;
using BuildingBlocks.Http.Auth;
using Api.Controllers.Business.OrdemServico;
using Api.Presenters.OrdemServico;
using Api.ViewModels.OrdemServico;
using Application.UseCases.Commands.AprovarOrdemServico;
using Application.UseCases.Commands.AprovarOrdemServicoPorToken;
using Application.UseCases.Commands.RejeitarOrdemServico;
using Application.UseCases.Commands.RejeitarOrdemServicoPorToken;
using Application.UseCases.Responses;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Api.Controllers.Business.OrdemServico;

public class OrdemServicoBusinessControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly OrdemServicoBusinessController _controller;

    public OrdemServicoBusinessControllerTests()
    {
        _controller = new OrdemServicoBusinessController(_mediator.Object, new OrdemServicoPresenter())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClienteJwtClaims.Documento, "86421537090")
                    ], authenticationType: "Cliente"))
                }
            }
        };
    }

    [Fact]
    public async Task Aprovar_ReturnsBadRequest_WhenTokenMissing()
    {
        // Arrange / Act
        var result = await _controller.Aprovar(" ");

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Aprovar_ReturnsOk_WhenTokenValid()
    {
        // Arrange
        _mediator
            .Setup(m => m.Send(It.IsAny<AprovarOrdemServicoPorTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AprovarOrdemServicoCommandResponse
            {
                Id = 1,
                Status = StatusOrdemServico.ChecandoEstoque,
                ValorTotal = 100m,
                RecebidaEm = DateTime.UtcNow,
                AprovadaEm = DateTime.UtcNow,
                Cliente = new ClienteOrdemServicoResponse { Id = 1, Nome = "Maria", Email = "maria@teste.com" },
                Veiculo = new VeiculoOrdemServicoResponse { Placa = "ABC1234", Marca = "VW", Modelo = "Gol" },
                Servicos = []
            });

        // Act
        var result = await _controller.Aprovar("token-abc");

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<AprovarOrdemServicoViewModel>(ok.Value);
        _mediator.Verify(
            m => m.Send(
                It.Is<AprovarOrdemServicoPorTokenCommand>(c =>
                    c.TokenAprovacao == "token-abc" && c.DocumentoCliente == "86421537090"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Rejeitar_ReturnsOk_WhenTokenValid()
    {
        // Arrange
        _mediator
            .Setup(m => m.Send(It.IsAny<RejeitarOrdemServicoPorTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RejeitarOrdemServicoCommandResponse
            {
                Id = 1,
                Status = StatusOrdemServico.EmDiagnostico,
                ValorTotal = 0m,
                RecebidaEm = DateTime.UtcNow,
                Cliente = new ClienteOrdemServicoResponse { Id = 1, Nome = "Maria", Email = "maria@teste.com" },
                Veiculo = new VeiculoOrdemServicoResponse { Placa = "ABC1234", Marca = "VW", Modelo = "Gol" },
                Servicos = []
            });

        // Act
        var result = await _controller.Rejeitar("token-abc");

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
}
