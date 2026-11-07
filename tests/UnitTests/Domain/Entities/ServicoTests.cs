using Domain.Exceptions;
using Domain.Entities;
using Domain.ValueObjects;

namespace UnitTests.Domain.Entities;

public class ServicoTests
{
    private ServicoCatalogo _servicoCatalogo = new ServicoCatalogo() { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
    
    
    [Fact]
    public void CreateServico_SetsSuggestedStatusAndProperties()
    {
        // Arrange & Act
        var servico = Servico.Criar("Troca de pastilhas", 200m, _servicoCatalogo);

        // Assert
        Assert.Equal(StatusItemOrdemServico.Sugerido, servico.Status);
        Assert.Equal("Troca de pastilhas", servico.Nome);
        Assert.Equal(200m, servico.ValorCobrado);
        Assert.Empty(servico.ItensNecessarios);
    }

    [Fact]
    public void Approve_SetsApprovedStatus()
    {
        // Arrange
        var servico = Servico.Criar("Troca de pastilhas", 200m, _servicoCatalogo);

        // Act
        servico.Aprovar();

        // Assert
        Assert.Equal(StatusItemOrdemServico.Aprovado, servico.Status);
        Assert.NotEqual(default, servico.AprovadoEm);
    }

    [Fact]
    public void Approve_WhenStatusIsNotSuggested_ThrowsBusinessRuleException()
    {
        // Arrange
        var servico = Servico.Criar("Troca de pastilhas", 200m, _servicoCatalogo);
        servico.Aprovar();

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => servico.Aprovar());
    }

    [Fact]
    public void Reject_SetsRejectedStatus()
    {
        // Arrange
        var servico = Servico.Criar("Diagnóstico", 100m, _servicoCatalogo);

        // Act
        servico.Rejeitar();

        // Assert
        Assert.Equal(StatusItemOrdemServico.Rejeitado, servico.Status);
        Assert.NotEqual(default, servico.RejeitadoEm);
    }

    [Fact]
    public void Reject_WhenStatusIsNotSuggested_ThrowsBusinessRuleException()
    {
        // Arrange
        var servico = Servico.Criar("Diagnóstico", 100m, _servicoCatalogo);
        servico.Rejeitar();

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => servico.Rejeitar());
    }

    [Fact]
    public void ConfirmConclusion_WhenApproved_SetsConcludedStatusWithoutConsumingStock()
    {
        // Arrange
        var servico = Servico.Criar("Troca de óleo", 150m, _servicoCatalogo);
        var itemEstoque = new ItemEstoqueOrdemServico { Id = 21, Nome = "Óleo" };
        servico.AdicionarItemNecessario(new ItemNecessario.CriarItemNecessarioParams(1, 2m, itemEstoque));
        servico.Aprovar();
        servico.ItensNecessarios.First().AplicarStatusEstoque(StatusItemEstoque.EstoqueTravado);

        // Act
        var iniciado = DateTime.UtcNow.AddMinutes(-30);
        var finalizado = DateTime.UtcNow;
        servico.ConfirmarConclusao(iniciado, finalizado);

        // Assert
        Assert.Equal(StatusItemOrdemServico.Concluido, servico.Status);
        Assert.Equal(iniciado, servico.ExecucaoIniciadaEm);
        Assert.Equal(finalizado, servico.ExecucaoFinalizadaEm);
        Assert.All(servico.ItensNecessarios, item => Assert.Equal(StatusItemEstoque.EstoqueTravado, item.Status));
    }

    [Fact]
    public void ConfirmConclusion_WhenNotApproved_ThrowsBusinessRuleException()
    {
        // Arrange
        var servico = Servico.Criar("Troca de óleo", 150m, _servicoCatalogo);

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => servico.ConfirmarConclusao(DateTime.UtcNow, DateTime.UtcNow));
    }
}
