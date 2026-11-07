using Domain.Exceptions;
using Domain.Entities;
using Domain.ValueObjects;

namespace UnitTests.Domain.Entities;

public class ItemNecessarioTests
{
    [Fact]
    public void CreateItemNecessario_SetsInitialStatusAndValues()
    {
        // Arrange
        var itemEstoque = new ItemEstoqueOrdemServico { Id = 10, Nome = "Filtro" };
        var parametro = new ItemNecessario.CriarItemNecessarioParams(1, 2m, itemEstoque);

        // Act
        var item = ItemNecessario.Criar(parametro);

        // Assert
        Assert.Equal(1, item.IdOrdemServico);
        Assert.Equal(2m, item.Quantidade);
        Assert.Equal(StatusItemEstoque.EstoqueNaoChecado, item.Status);
        Assert.Equal(itemEstoque, item.ItemEstoque);
    }

    [Fact]
    public void AplicarStatusEstoque_WhenAvailable_SetsAvailableStatus()
    {
        // Arrange
        var itemEstoque = new ItemEstoqueOrdemServico { Id = 11, Nome = "Óleo" };
        var item = ItemNecessario.Criar(new ItemNecessario.CriarItemNecessarioParams(1, 3m, itemEstoque));

        // Act
        item.AplicarStatusEstoque(StatusItemEstoque.EstoqueDisponivel);

        // Assert
        Assert.Equal(StatusItemEstoque.EstoqueDisponivel, item.Status);
    }

    [Fact]
    public void AplicarStatusEstoque_WhenMissing_SetsOutOfStockStatus()
    {
        // Arrange
        var itemEstoque = new ItemEstoqueOrdemServico { Id = 11, Nome = "Óleo" };
        var item = ItemNecessario.Criar(new ItemNecessario.CriarItemNecessarioParams(1, 10m, itemEstoque));

        // Act
        item.AplicarStatusEstoque(StatusItemEstoque.EstoqueEmFalta);

        // Assert
        Assert.Equal(StatusItemEstoque.EstoqueEmFalta, item.Status);
    }

    [Fact]
    public void AplicarStatusEstoque_WhenStatusIsUtilizado_ThrowsBusinessRuleException()
    {
        // Arrange
        var item = ItemNecessario.Criar(new ItemNecessario.CriarItemNecessarioParams(1, 2m, new ItemEstoqueOrdemServico { Id = 12, Nome = "Parafuso" }));
        item.AplicarStatusEstoque(StatusItemEstoque.EstoqueTravado);
        item.ConfirmarUtilizacao();

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => item.AplicarStatusEstoque(StatusItemEstoque.EstoqueDisponivel));
    }

    [Fact]
    public void AplicarStatusEstoque_WhenLocked_SetsLockedStatus()
    {
        // Arrange
        var item = ItemNecessario.Criar(new ItemNecessario.CriarItemNecessarioParams(1, 2m, new ItemEstoqueOrdemServico { Id = 12, Nome = "Parafuso" }));

        // Act
        item.AplicarStatusEstoque(StatusItemEstoque.EstoqueTravado);

        // Assert
        Assert.Equal(StatusItemEstoque.EstoqueTravado, item.Status);
    }

    [Fact]
    public void AplicarStatusEstoque_WhenStatusIsInvalid_ThrowsBusinessRuleException()
    {
        // Arrange
        var item = ItemNecessario.Criar(new ItemNecessario.CriarItemNecessarioParams(1, 2m, new ItemEstoqueOrdemServico { Id = 13, Nome = "Correia" }));

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => item.AplicarStatusEstoque(StatusItemEstoque.Utilizado));
    }

    [Fact]
    public void ConfirmUsage_WhenStatusIsLocked_SetsUtilizedStatus()
    {
        // Arrange
        var item = ItemNecessario.Criar(new ItemNecessario.CriarItemNecessarioParams(1, 2m, new ItemEstoqueOrdemServico { Id = 13, Nome = "Correia" }));
        item.AplicarStatusEstoque(StatusItemEstoque.EstoqueTravado);

        // Act
        item.ConfirmarUtilizacao();

        // Assert
        Assert.Equal(StatusItemEstoque.Utilizado, item.Status);
    }

    [Fact]
    public void ConfirmUsage_WhenNotLocked_ThrowsBusinessRuleException()
    {
        // Arrange
        var item = ItemNecessario.Criar(new ItemNecessario.CriarItemNecessarioParams(1, 1m, new ItemEstoqueOrdemServico { Id = 14, Nome = "Filtro" }));

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => item.ConfirmarUtilizacao());
    }
}
