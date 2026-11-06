using Api.Controllers.Staff.OrdemServico.AdicionarItemServico;
using Api.Controllers.Staff.OrdemServico.AprovarServicosParcialmente;
using Api.Controllers.Staff.OrdemServico.ConfirmarExecucao;
using Api.Controllers.Staff.OrdemServico.CriarOrdemServico;
using Domain.ValueObjects;

namespace UnitTests.Api.Controllers.Staff.OrdemServico;

public class OrdemServicoRequestValidatorTests
{
    [Fact]
    public void Criar_IsValid_WhenIdVeiculoIsPositive()
    {
        // Arrange
        var validator = new CriarOrdemServicoRequestValidator();
        var request = new CriarOrdemServicoRequest { IdVeiculo = 1 };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Criar_HasError_WhenIdVeiculoIsInvalid()
    {
        // Arrange
        var validator = new CriarOrdemServicoRequestValidator();
        var request = new CriarOrdemServicoRequest { IdVeiculo = 0 };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("IdVeiculo deve ser um veículo válido.", result.Errors);
    }

    [Fact]
    public void Criar_IsValid_WhenServicosListIsEmpty()
    {
        // Arrange
        var validator = new CriarOrdemServicoRequestValidator();
        var request = new CriarOrdemServicoRequest { IdVeiculo = 1, Servicos = [] };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Criar_HasErrors_WhenServicoFieldsAreInvalid()
    {
        // Arrange
        var validator = new CriarOrdemServicoRequestValidator();
        var request = new CriarOrdemServicoRequest
        {
            IdVeiculo = 1,
            Servicos =
            [
                new CriarOrdemServicoRequest.Servico
                {
                    IdServico = 0,
                    ValorCobrado = 0m,
                    ItensNecessarios =
                    [
                        new CriarOrdemServicoRequest.ItemNecessario { IdItemEstoque = 0, Quantidade = 0m }
                    ]
                }
            ]
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 4);
    }

    [Fact]
    public void AdicionarItem_IsValid_WhenRequestIsComplete()
    {
        // Arrange
        var validator = new AdicionarItemServicoRequestValidator();
        var request = new AdicionarItemServicoRequest
        {
            IdServico = 1,
            ValorCobrado = 100m,
            ItensNecessarios =
            [
                new AdicionarItemServicoRequest.ItemNecessario { IdItemEstoque = 1, Quantidade = 2m }
            ]
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void AdicionarItem_HasErrors_WhenFieldsAreInvalid()
    {
        // Arrange
        var validator = new AdicionarItemServicoRequestValidator();
        var request = new AdicionarItemServicoRequest
        {
            IdServico = 0,
            ValorCobrado = 0m,
            ItensNecessarios =
            [
                new AdicionarItemServicoRequest.ItemNecessario { IdItemEstoque = 0, Quantidade = 0m }
            ]
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 4);
    }

    [Fact]
    public void AprovarParcialmente_IsValid_WhenHasIds()
    {
        // Arrange
        var validator = new AprovarServicosParcialmenteRequestValidator();
        var request = new AprovarServicosParcialmenteRequest { IdsServicosAprovados = [1, 2] };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void AprovarParcialmente_HasError_WhenListIsEmpty()
    {
        // Arrange
        var validator = new AprovarServicosParcialmenteRequestValidator();
        var request = new AprovarServicosParcialmenteRequest { IdsServicosAprovados = [] };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Pelo menos um serviço deve ser aprovado.", result.Errors);
    }

    [Fact]
    public void ConfirmarExecucao_IsValid_WhenServicosAreValid()
    {
        // Arrange
        var validator = new ConfirmarExecucaoRequestValidator();
        var iniciado = DateTime.UtcNow.AddHours(-1);
        var request = new ConfirmarExecucaoRequest
        {
            ServicosExecutados =
            [
                new ServicoExecutado { IdServico = 1, IniciadoEm = iniciado, FinalizadoEm = DateTime.UtcNow }
            ]
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ConfirmarExecucao_HasError_WhenListIsEmpty()
    {
        // Arrange
        var validator = new ConfirmarExecucaoRequestValidator();
        var request = new ConfirmarExecucaoRequest { ServicosExecutados = [] };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Pelo menos um serviço deve ter sido executado.", result.Errors);
    }

    [Fact]
    public void ConfirmarExecucao_HasErrors_WhenItemFieldsAreInvalid()
    {
        // Arrange
        var validator = new ConfirmarExecucaoRequestValidator();
        var agora = DateTime.UtcNow;
        var request = new ConfirmarExecucaoRequest
        {
            ServicosExecutados =
            [
                new ServicoExecutado { IdServico = 0, IniciadoEm = agora, FinalizadoEm = agora.AddMinutes(-5) }
            ]
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 2);
    }
}
