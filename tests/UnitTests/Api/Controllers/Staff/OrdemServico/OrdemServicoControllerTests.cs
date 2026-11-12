using UnitTests.Support;
using Api.Contracts.Validation;
using Domain.Exceptions;
using Api.Controllers.Staff.OrdemServico;
using Api.Controllers.Staff.OrdemServico.AdicionarItemServico;
using Api.Controllers.Staff.OrdemServico.AprovarServicosParcialmente;
using Api.Controllers.Staff.OrdemServico.ConfirmarExecucao;
using Api.Controllers.Staff.OrdemServico.CriarOrdemServico;
using Api.Presenters.OrdemServico;
using Api.ViewModels.OrdemServico;
using Application.UseCases;
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
using Application.UseCases.Responses;
using Domain.Entities;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Api.Controllers.Staff.OrdemServico;

public class OrdemServicoControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly Mock<IValidator<CriarOrdemServicoRequest>> _criarValidatorMock = new();
    private readonly Mock<IValidator<AdicionarItemServicoRequest>> _adicionarValidatorMock = new();
    private readonly Mock<IValidator<AprovarServicosParcialmenteRequest>> _aprovarParcialValidatorMock = new();
    private readonly Mock<IValidator<ConfirmarExecucaoRequest>> _confirmarExecucaoValidatorMock = new();
    private readonly OrdemServicoController _controller;

    public OrdemServicoControllerTests()
    {
        _controller = new OrdemServicoController(
            _mediatorMock.Object,
            new OrdemServicoPresenter(),
            _criarValidatorMock.Object,
            _adicionarValidatorMock.Object,
            _aprovarParcialValidatorMock.Object,
            _confirmarExecucaoValidatorMock.Object);
    }

    [Fact]
    public async Task Listar_ReturnsOk()
    {
        // Arrange
        var response = new OrdemServicoResponse
        {
            Id = OrdemServicoTestIds.Of(1),
            Status = StatusOrdemServico.Recebida,
            ValorTotal = 0m,
            RecebidaEm = DateTime.UtcNow,
            Cliente = new ClienteOrdemServicoResponse { Id = 1, Nome = "Maria", Email = "maria@teste.com" },
            Veiculo = new VeiculoOrdemServicoResponse { Placa = "ABC-1234", Marca = "VW", Modelo = "Gol" },
            Servicos = [],
            ItensNecessariosParaExecucao = []
        };
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ListarOrdensServicoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrdemServicoResponse> { response });

        // Act
        var result = await _controller.Listar();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var viewModels = Assert.IsAssignableFrom<IReadOnlyList<OrdemServicoViewModel>>(ok.Value);
        Assert.Single(viewModels);
        Assert.Equal(response.Id, viewModels[0].Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenOrdemDoesNotExist()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOrdemServicoByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemServicoResponse?)null);

        // Act
        var result = await _controller.GetById(OrdemServicoTestIds.Of(999));

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenOrdemExists()
    {
        // Arrange
        var response = new OrdemServicoResponse
        {
            Id = OrdemServicoTestIds.Of(1),
            Status = StatusOrdemServico.Recebida,
            ValorTotal = 0m,
            RecebidaEm = DateTime.UtcNow,
            Cliente = new ClienteOrdemServicoResponse { Id = 1, Nome = "Maria", Email = "maria@teste.com" },
            Veiculo = new VeiculoOrdemServicoResponse { Placa = "ABC-1234", Marca = "VW", Modelo = "Gol" },
            Servicos = [],
            ItensNecessariosParaExecucao = []
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOrdemServicoByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetById(OrdemServicoTestIds.Of(1));

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var viewModel = Assert.IsType<OrdemServicoViewModel>(ok.Value);
        Assert.Equal(response.Id, viewModel.Id);
        Assert.Equal(response.Status, viewModel.Status);
    }

    [Fact]
    public async Task CriarOrdemServico_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var validation = new ValidationResult();
        validation.Errors.Add("IdVeiculo deve ser um veículo válido.");
        _criarValidatorMock.Setup(v => v.Validate(It.IsAny<CriarOrdemServicoRequest>())).Returns(validation);

        // Act
        var result = await _controller.CriarOrdemServico(new CriarOrdemServicoRequest { IdVeiculo = 0 });

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CriarOrdemServico_ReturnsCreated_WhenRequestIsValid()
    {
        // Arrange
        _criarValidatorMock.Setup(v => v.Validate(It.IsAny<CriarOrdemServicoRequest>())).Returns(new ValidationResult());

        var response = new CriarOrdemServicoCommandResponse
        {
            Id = OrdemServicoTestIds.Of(1),
            Status = StatusOrdemServico.Recebida,
            ValorTotal = 0m,
            RecebidaEm = DateTime.UtcNow,
            Cliente = new ClienteOrdemServicoResponse { Id = 1, Nome = "Maria", Email = "maria@teste.com" },
            Veiculo = new VeiculoOrdemServicoResponse { Placa = "ABC-1234", Marca = "VW", Modelo = "Gol" },
            Servicos = []
        };

        CriarOrdemServicoCommand? captured = null;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CriarOrdemServicoCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<CriarOrdemServicoCommandResponse>, CancellationToken>((cmd, _) =>
                captured = (CriarOrdemServicoCommand)cmd)
            .ReturnsAsync(response);

        var request = new CriarOrdemServicoRequest
        {
            IdVeiculo = 1,
            Servicos =
            [
                new CriarOrdemServicoRequest.Servico
                {
                    IdServico = 10,
                    ValorCobrado = 150m,
                    ItensNecessarios =
                    [
                        new CriarOrdemServicoRequest.ItemNecessario { IdItemEstoque = 100, Quantidade = 2m }
                    ]
                }
            ]
        };

        // Act
        var result = await _controller.CriarOrdemServico(request);

        // Assert
        var created = Assert.IsType<CreatedResult>(result);
        var viewModel = Assert.IsType<CriarOrdemServicoViewModel>(created.Value);
        Assert.Equal(response.Id, viewModel.Id);
        Assert.Equal(response.Status, viewModel.Status);
        Assert.NotNull(captured);
        Assert.Single(captured.Servicos);
        Assert.Equal(10, captured.Servicos[0].IdServico);
        Assert.Single(captured.Servicos[0].ItensNecessarios);
        Assert.Equal(100, captured.Servicos[0].ItensNecessarios[0].IdItemEstoque);
        Assert.Equal(2m, captured.Servicos[0].ItensNecessarios[0].Quantidade);
    }

    [Fact]
    public async Task DescartarOrdemServico_ThrowsDomainNotFoundException_WhenNotFound()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DescartarOrdemServicoCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainNotFoundException("não encontrada"));

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() => _controller.DescartarOrdemServico(OrdemServicoTestIds.Of(999)));
    }

    [Fact]
    public async Task AdicionarItemServico_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var validation = new ValidationResult();
        validation.Errors.Add("IdServico inválido");
        _adicionarValidatorMock.Setup(v => v.Validate(It.IsAny<AdicionarItemServicoRequest>())).Returns(validation);

        // Act
        var result = await _controller.AdicionarItemServico(OrdemServicoTestIds.Of(1), new AdicionarItemServicoRequest
        {
            IdServico = 0,
            ValorCobrado = 0m
        });

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AdicionarItemServico_ReturnsOk_WhenValid()
    {
        // Arrange
        _adicionarValidatorMock.Setup(v => v.Validate(It.IsAny<AdicionarItemServicoRequest>())).Returns(new ValidationResult());

        var response = new AdicionarItemOrdemServicoCommandResponse
        {
            Id = OrdemServicoTestIds.Of(1),
            Status = StatusOrdemServico.EmDiagnostico,
            ValorTotal = 100m,
            RecebidaEm = DateTime.UtcNow,
            Cliente = new ClienteOrdemServicoResponse { Id = 1, Nome = "Maria", Email = "maria@teste.com" },
            Veiculo = new VeiculoOrdemServicoResponse { Placa = "ABC-1234", Marca = "VW", Modelo = "Gol" },
            Itens = []
        };

        AdicionarItemOrdemServicoCommand? captured = null;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<AdicionarItemOrdemServicoCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<AdicionarItemOrdemServicoCommandResponse>, CancellationToken>((cmd, _) =>
                captured = (AdicionarItemOrdemServicoCommand)cmd)
            .ReturnsAsync(response);

        var request = new AdicionarItemServicoRequest
        {
            IdServico = 1,
            ValorCobrado = 100m,
            ItensNecessarios =
            [
                new AdicionarItemServicoRequest.ItemNecessario { IdItemEstoque = 55, Quantidade = 3m }
            ]
        };

        // Act
        var result = await _controller.AdicionarItemServico(OrdemServicoTestIds.Of(1), request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(captured);
        Assert.Single(captured.ItensNecessarios);
        Assert.Equal(55, captured.ItensNecessarios[0].IdItemEstoque);
        Assert.Equal(3m, captured.ItensNecessarios[0].Quantidade);
    }

    [Fact]
    public async Task FinalizarDiagnostico_ReturnsOk()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<FinalizarDiagnosticoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FinalizarDiagnosticoCommandResponse
            {
                Id = OrdemServicoTestIds.Of(1),
                Status = StatusOrdemServico.AguardandoAprovacao,
                ValorTotal = 100m,
                RecebidaEm = DateTime.UtcNow,
                Cliente = new ClienteOrdemServicoResponse { Id = 1, Nome = "Maria", Email = "m@t.com" },
                Veiculo = new VeiculoOrdemServicoResponse { Placa = "ABC", Marca = "VW", Modelo = "Gol" },
                Servicos = []
            });

        // Act
        var result = await _controller.FinalizarDiagnostico(OrdemServicoTestIds.Of(1));

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Aprovar_ReturnsOk()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<AprovarOrdemServicoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AprovarOrdemServicoCommandResponse
            {
                Id = OrdemServicoTestIds.Of(1),
                Status = StatusOrdemServico.ChecandoEstoque,
                ValorTotal = 100m,
                RecebidaEm = DateTime.UtcNow,
                AprovadaEm = DateTime.UtcNow,
                Cliente = new ClienteOrdemServicoResponse { Id = 1, Nome = "Maria", Email = "m@t.com" },
                Veiculo = new VeiculoOrdemServicoResponse { Placa = "ABC", Marca = "VW", Modelo = "Gol" },
                Servicos = []
            });

        // Act
        var result = await _controller.AprovarOrdemServico(OrdemServicoTestIds.Of(1));

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Rejeitar_ReturnsOk()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RejeitarOrdemServicoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RejeitarOrdemServicoCommandResponse
            {
                Id = OrdemServicoTestIds.Of(1),
                Status = StatusOrdemServico.EmDiagnostico,
                ValorTotal = 0m,
                RecebidaEm = DateTime.UtcNow,
                EntregueEm = DateTime.UtcNow,
                Cliente = new ClienteOrdemServicoResponse { Id = 1, Nome = "Maria", Email = "m@t.com" },
                Veiculo = new VeiculoOrdemServicoResponse { Placa = "ABC", Marca = "VW", Modelo = "Gol" },
                Servicos = []
            });

        // Act
        var result = await _controller.RejeitarOrdemServico(OrdemServicoTestIds.Of(1));

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetTempoMedioExecucao_ReturnsOk()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetTempoMedioExecucaoAllServicosQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                TempoMedioExecucaoResponse.From(1, 2, TimeSpan.FromHours(1.5))
            ]);

        // Act
        var result = await _controller.GetTempoMedioExecucao();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var viewModels = Assert.IsAssignableFrom<IEnumerable<TempoMedioExecucaoViewModel>>(ok.Value);
        var item = Assert.Single(viewModels);
        Assert.Equal(1, item.IdServico);
        Assert.Equal(2, item.TotalExecucoes);
    }

    [Fact]
    public async Task AprovarServicosParcialmente_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var validation = new ValidationResult();
        validation.Errors.Add("erro");
        _aprovarParcialValidatorMock.Setup(v => v.Validate(It.IsAny<AprovarServicosParcialmenteRequest>())).Returns(validation);

        // Act
        var result = await _controller.AprovarServicosParcialmente(OrdemServicoTestIds.Of(1), new AprovarServicosParcialmenteRequest());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AprovarServicosParcialmente_ReturnsOk_WhenValid()
    {
        // Arrange
        _aprovarParcialValidatorMock.Setup(v => v.Validate(It.IsAny<AprovarServicosParcialmenteRequest>())).Returns(new ValidationResult());
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<AprovarServicosParcialmenteCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AprovarServicosParcialmenteCommandResponse
            {
                Id = OrdemServicoTestIds.Of(1),
                Status = StatusOrdemServico.EmDiagnostico,
                ValorTotal = 100m,
                RecebidaEm = DateTime.UtcNow,
                Cliente = new ClienteOrdemServicoResponse { Id = 1, Nome = "Maria", Email = "m@t.com" },
                Veiculo = new VeiculoOrdemServicoResponse { Placa = "ABC", Marca = "VW", Modelo = "Gol" },
                Servicos = []
            });

        // Act
        var result = await _controller.AprovarServicosParcialmente(OrdemServicoTestIds.Of(1), new AprovarServicosParcialmenteRequest { IdsServicosAprovados = [1] });

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task AprovarServicosParcialmente_ThrowsException_WhenMediatorThrows()
    {
        // Arrange
        _aprovarParcialValidatorMock.Setup(v => v.Validate(It.IsAny<AprovarServicosParcialmenteRequest>())).Returns(new ValidationResult());
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<AprovarServicosParcialmenteCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("x"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _controller.AprovarServicosParcialmente(OrdemServicoTestIds.Of(1), new AprovarServicosParcialmenteRequest { IdsServicosAprovados = [1] }));
    }

    [Fact]
    public async Task ConfirmarExecucao_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var validation = new ValidationResult();
        validation.Errors.Add("erro");
        _confirmarExecucaoValidatorMock.Setup(v => v.Validate(It.IsAny<ConfirmarExecucaoRequest>())).Returns(validation);

        // Act
        var result = await _controller.ConfirmarExecucao(OrdemServicoTestIds.Of(1), new ConfirmarExecucaoRequest());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ConfirmarExecucao_ReturnsOk_WhenValid()
    {
        // Arrange
        _confirmarExecucaoValidatorMock.Setup(v => v.Validate(It.IsAny<ConfirmarExecucaoRequest>())).Returns(new ValidationResult());
        var response = new OrdemServicoResponse
        {
            Id = OrdemServicoTestIds.Of(1),
            Status = StatusOrdemServico.Finalizada,
            ValorTotal = 100m,
            RecebidaEm = DateTime.UtcNow,
            Cliente = new ClienteOrdemServicoResponse { Id = 1, Nome = "Maria", Email = "m@t.com" },
            Veiculo = new VeiculoOrdemServicoResponse { Placa = "ABC", Marca = "VW", Modelo = "Gol" },
            Servicos = [],
            ItensNecessariosParaExecucao = []
        };
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ConfirmarExecucaoOrdemServicoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ConfirmarExecucao(OrdemServicoTestIds.Of(1), new ConfirmarExecucaoRequest
        {
            ServicosExecutados = [new ServicoExecutado { IdServico = 1, IniciadoEm = DateTime.UtcNow.AddHours(-1), FinalizadoEm = DateTime.UtcNow }]
        });

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var viewModel = Assert.IsType<OrdemServicoViewModel>(ok.Value);
        Assert.Equal(response.Id, viewModel.Id);
        Assert.Equal(response.Status, viewModel.Status);
    }

    [Fact]
    public async Task ConfirmarExecucao_ThrowsException_WhenMediatorThrows()
    {
        // Arrange
        _confirmarExecucaoValidatorMock.Setup(v => v.Validate(It.IsAny<ConfirmarExecucaoRequest>())).Returns(new ValidationResult());
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ConfirmarExecucaoOrdemServicoCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("x"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _controller.ConfirmarExecucao(OrdemServicoTestIds.Of(1), new ConfirmarExecucaoRequest
            {
                ServicosExecutados = [new ServicoExecutado { IdServico = 1, IniciadoEm = DateTime.UtcNow.AddHours(-1), FinalizadoEm = DateTime.UtcNow }]
            }));
    }

    [Fact]
    public async Task Descartar_ReturnsOk_WhenValid()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DescartarOrdemServicoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DescartarOrdemServicoResponse
            {
                Id = OrdemServicoTestIds.Of(1),
                Status = StatusOrdemServico.Descartada,
                RecebidaEm = DateTime.UtcNow,
                DescartadaEm = DateTime.UtcNow,
                Itens = [],
                Cliente = new ClienteOrdemServicoResponse { Id = 1, Nome = "Maria", Email = "m@t.com" },
                Veiculo = new VeiculoOrdemServicoResponse { Placa = "ABC", Marca = "VW", Modelo = "Gol" }
            });

        // Act
        var result = await _controller.DescartarOrdemServico(OrdemServicoTestIds.Of(1));

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task FinalizarDiagnostico_ThrowsException_WhenMediatorThrows()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<FinalizarDiagnosticoCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("x"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.FinalizarDiagnostico(OrdemServicoTestIds.Of(1)));
    }

    [Fact]
    public async Task GetTempoMedioExecucao_ThrowsException_WhenMediatorThrows()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetTempoMedioExecucaoAllServicosQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("x"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetTempoMedioExecucao());
    }
}
