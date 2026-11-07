using Api.Contracts.Validation;
using Domain.Exceptions;
using Api.Controllers.Staff.ItemEstoque;
using Api.Controllers.Staff.ItemEstoque.CreateItemEstoque;
using Api.Controllers.Staff.ItemEstoque.RegistrarEntradaEstoque;
using Api.Controllers.Staff.ItemEstoque.UpdateItemEstoque;
using Api.Presenters.ItemEstoque;
using Api.ViewModels.ItemEstoque;
using Application.UseCases.Estoque.ItemEstoque.Commands.CreateItemEstoque;
using Application.UseCases.Estoque.ItemEstoque.Responses;
using Application.UseCases.Estoque.ItemEstoque.Commands.DeleteItemEstoque;
using Application.UseCases.Estoque.ItemEstoque.Commands.RegistrarEntradaEstoque;
using Application.UseCases.Estoque.ItemEstoque.Commands.UpdateItemEstoque;
using Application.UseCases.Estoque.ItemEstoque.Queries.GetAllItensEstoque;
using Application.UseCases.Estoque.ItemEstoque.Queries.GetItemEstoqueById;
using Domain.Estoque.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Api.Controllers.Staff.ItemEstoque;

public class ItemEstoqueControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly Mock<IValidator<CreateItemEstoqueRequest>> _createValidatorMock = new();
    private readonly Mock<IValidator<UpdateItemEstoqueRequest>> _updateValidatorMock = new();
    private readonly Mock<IValidator<RegistrarEntradaEstoqueRequest>> _registrarValidatorMock = new();
    private readonly ItemEstoqueController _controller;

    public ItemEstoqueControllerTests()
    {
        _controller = new ItemEstoqueController(
            _mediatorMock.Object,
            new ItemEstoquePresenter(),
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            _registrarValidatorMock.Object);
    }

    private static ItemEstoqueResponse SampleResponse() => new()
    {
        Id = 1,
        Codigo = "ITM-001",
        Tipo = ItemTipo.Peca,
        Nome = "Filtro",
        UnidadeMedida = UnidadeMedida.Unidade,
        PrecoVenda = 55.5m,
        Saldo = 10m,
        SaldoReservado = 0m
    };

    [Fact]
    public async Task Create_ReturnsCreated_WhenRequestIsValid()
    {
        // Arrange
        _createValidatorMock.Setup(v => v.Validate(It.IsAny<CreateItemEstoqueRequest>())).Returns(new ValidationResult());
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateItemEstoqueCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleResponse());

        // Act
        var result = await _controller.Create(new CreateItemEstoqueRequest { Codigo = "ITM-001", Nome = "Filtro", PrecoVenda = 1m });

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var viewModel = Assert.IsType<ItemEstoqueViewModel>(createdResult.Value);
        Assert.Equal(1, viewModel.Id);
        Assert.Equal("ITM-001", viewModel.Codigo);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var validation = new ValidationResult();
        validation.Errors.Add("erro");
        _createValidatorMock.Setup(v => v.Validate(It.IsAny<CreateItemEstoqueRequest>())).Returns(validation);

        // Act
        var result = await _controller.Create(new CreateItemEstoqueRequest());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_ThrowsInvalidOperationException_WhenMediatorThrows()
    {
        // Arrange
        _createValidatorMock.Setup(v => v.Validate(It.IsAny<CreateItemEstoqueRequest>())).Returns(new ValidationResult());
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateItemEstoqueCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("falha"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _controller.Create(new CreateItemEstoqueRequest { Codigo = "X", Nome = "Y", PrecoVenda = 1m }));
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetItemEstoqueByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ItemEstoqueResponse?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenItemExists()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetItemEstoqueByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleResponse());

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var viewModel = Assert.IsType<ItemEstoqueViewModel>(okResult.Value);
        Assert.Equal(1, viewModel.Id);
        Assert.Equal("ITM-001", viewModel.Codigo);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllItemEstoqueQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ItemEstoqueResponse> { SampleResponse() });

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var viewModels = Assert.IsAssignableFrom<IEnumerable<ItemEstoqueViewModel>>(okResult.Value).ToList();
        Assert.Single(viewModels);
        Assert.Equal(1, viewModels[0].Id);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var validation = new ValidationResult();
        validation.Errors.Add("erro");
        _updateValidatorMock.Setup(v => v.Validate(It.IsAny<UpdateItemEstoqueRequest>())).Returns(validation);

        // Act
        var result = await _controller.Update(1, new UpdateItemEstoqueRequest());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenValid()
    {
        // Arrange
        _updateValidatorMock.Setup(v => v.Validate(It.IsAny<UpdateItemEstoqueRequest>())).Returns(new ValidationResult());
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateItemEstoqueCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleResponse());

        // Act
        var result = await _controller.Update(1, new UpdateItemEstoqueRequest { Codigo = "A", Nome = "B", PrecoVenda = 1m });

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var viewModel = Assert.IsType<ItemEstoqueViewModel>(okResult.Value);
        Assert.Equal("ITM-001", viewModel.Codigo);
    }

    [Fact]
    public async Task Update_ThrowsDomainNotFoundException_WhenNotFound()
    {
        // Arrange
        _updateValidatorMock.Setup(v => v.Validate(It.IsAny<UpdateItemEstoqueRequest>())).Returns(new ValidationResult());
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateItemEstoqueCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainNotFoundException("Item não encontrado"));

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            _controller.Update(1, new UpdateItemEstoqueRequest { Codigo = "A", Nome = "B", PrecoVenda = 1m }));
    }

    [Fact]
    public async Task Update_ThrowsException_WhenOtherException()
    {
        // Arrange
        _updateValidatorMock.Setup(v => v.Validate(It.IsAny<UpdateItemEstoqueRequest>())).Returns(new ValidationResult());
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateItemEstoqueCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("x"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _controller.Update(1, new UpdateItemEstoqueRequest { Codigo = "A", Nome = "B", PrecoVenda = 1m }));
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleted()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteItemEstoqueCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ThrowsDomainNotFoundException_WhenNotFound()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteItemEstoqueCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainNotFoundException("Item não encontrado"));

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() => _controller.Delete(999));
    }

    [Fact]
    public async Task Delete_ThrowsException_WhenOtherException()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteItemEstoqueCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("x"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Delete(1));
    }

    [Fact]
    public async Task RegistrarEntrada_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var validation = new ValidationResult();
        validation.Errors.Add("erro");
        _registrarValidatorMock.Setup(v => v.Validate(It.IsAny<RegistrarEntradaEstoqueRequest>())).Returns(validation);

        // Act
        var result = await _controller.RegistrarEntrada(1, new RegistrarEntradaEstoqueRequest());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task RegistrarEntrada_ReturnsOk_WhenValid()
    {
        // Arrange
        _registrarValidatorMock.Setup(v => v.Validate(It.IsAny<RegistrarEntradaEstoqueRequest>())).Returns(new ValidationResult());
        _mediatorMock.Setup(m => m.Send(It.IsAny<RegistrarEntradaEstoqueCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleResponse());

        // Act
        var result = await _controller.RegistrarEntrada(1, new RegistrarEntradaEstoqueRequest { Quantidade = 5m });

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var viewModel = Assert.IsType<ItemEstoqueViewModel>(okResult.Value);
        Assert.Equal(1, viewModel.Id);
        Assert.Equal(10m, viewModel.Saldo);
    }

    [Fact]
    public async Task RegistrarEntrada_ThrowsException_WhenOtherException()
    {
        // Arrange
        _registrarValidatorMock.Setup(v => v.Validate(It.IsAny<RegistrarEntradaEstoqueRequest>())).Returns(new ValidationResult());
        _mediatorMock.Setup(m => m.Send(It.IsAny<RegistrarEntradaEstoqueCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("x"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _controller.RegistrarEntrada(1, new RegistrarEntradaEstoqueRequest { Quantidade = 5m }));
    }
}
