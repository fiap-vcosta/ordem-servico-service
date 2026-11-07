using Api.Controllers.Staff.ItemEstoque.CreateItemEstoque;
using Api.Controllers.Staff.ItemEstoque.RegistrarEntradaEstoque;
using Api.Controllers.Staff.ItemEstoque.UpdateItemEstoque;
using Domain.Estoque.Entities;

namespace UnitTests.Api.Controllers.Staff.ItemEstoque;

public class ItemEstoqueRequestValidatorTests
{
    [Fact]
    public void Create_IsValid_WhenRequestIsComplete()
    {
        // Arrange
        var validator = new CreateItemEstoqueRequestValidator();
        var request = new CreateItemEstoqueRequest
        {
            Codigo = "ITM-001",
            Tipo = ItemTipo.Peca,
            Nome = "Filtro",
            UnidadeMedida = UnidadeMedida.Unidade,
            PrecoVenda = 10m,
            Saldo = 5m,
            SaldoReservado = 0m
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Create_HasErrors_WhenRequiredFieldsAreInvalid()
    {
        // Arrange
        var validator = new CreateItemEstoqueRequestValidator();
        var request = new CreateItemEstoqueRequest
        {
            Codigo = "",
            Tipo = (ItemTipo)99,
            Nome = "",
            UnidadeMedida = (UnidadeMedida)99,
            PrecoVenda = 0m,
            SaldoReservado = -1m
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Código não pode estar vazio.", result.Errors);
        Assert.Contains("Tipo de item inválido.", result.Errors);
        Assert.Contains("Nome não pode estar vazio.", result.Errors);
        Assert.Contains("Unidade de medida inválida.", result.Errors);
        Assert.Contains("Preço de venda deve ser maior que zero.", result.Errors);
        Assert.Contains("Saldo reservado não pode ser negativo.", result.Errors);
    }

    [Fact]
    public void Update_IsValid_WhenRequestIsComplete()
    {
        // Arrange
        var validator = new UpdateItemEstoqueRequestValidator();
        var request = new UpdateItemEstoqueRequest
        {
            Codigo = "ITM-002",
            Tipo = (int)ItemTipo.Insumo,
            Nome = "Óleo",
            UnidadeMedida = (int)UnidadeMedida.Litro,
            PrecoVenda = 25m
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Update_HasErrors_WhenRequiredFieldsAreInvalid()
    {
        // Arrange
        var validator = new UpdateItemEstoqueRequestValidator();
        var request = new UpdateItemEstoqueRequest
        {
            Codigo = " ",
            Tipo = 99,
            Nome = " ",
            UnidadeMedida = 99,
            PrecoVenda = -1m
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 4);
    }

    [Fact]
    public void RegistrarEntrada_IsValid_WhenQuantidadeIsPositive()
    {
        // Arrange
        var validator = new RegistrarEntradaEstoqueRequestValidator();
        var request = new RegistrarEntradaEstoqueRequest { Quantidade = 10m };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void RegistrarEntrada_HasError_WhenQuantidadeIsNotPositive()
    {
        // Arrange
        var validator = new RegistrarEntradaEstoqueRequestValidator();
        var request = new RegistrarEntradaEstoqueRequest { Quantidade = 0m };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }
}
