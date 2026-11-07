using IntegrationTests.Infrastructure;

namespace IntegrationTests.OrdemServico;

[Collection(nameof(IntegrationFixture))]
public class ItensEmFaltaAteEntregueIntegrationTests(CustomWebApplicationFactory factory)
    : OrdemServicoIntegrationTestBase(factory)
{
    [Fact]
    public async Task Fluxo_AprovarInternamente_ItemEmFalta_DepoisReservar_AteEntregue()
    {
        // Arrange
        AuthenticateAsStaff();
        const int idItemEmFalta = 100;

        // Act
        var ordemId = await CriarOrdemAsync(
            idVeiculo: 5,
            servicos:
            [
                new
                {
                    IdServico = 1,
                    ValorCobrado = 150m,
                    ItensNecessarios = new[] { new { IdItemEstoque = idItemEmFalta, Quantidade = 1m } }
                }
            ]);
        await FinalizarDiagnosticoEAplicarOrcamentoAsync(ordemId);
        await AprovarInternamenteAsync(ordemId);
        await AssertStatusAsync(ordemId, "ChecandoEstoque");

        await AplicarEstoqueEmFaltaAsync(ordemId, idItemEmFalta);
        await AssertStatusAsync(ordemId, "AguardandoPeca");

        // Assert
        await ExecutarAteEntregueAsync(ordemId);
    }

    [Fact]
    public async Task Fluxo_AprovarPublicamente_ItemEmFalta_DepoisReservar_AteEntregue()
    {
        // Arrange
        AuthenticateAsStaff();
        const int idItemEmFalta = 100;

        // Act
        var ordemId = await CriarOrdemAsync(
            idVeiculo: 5,
            servicos:
            [
                new
                {
                    IdServico = 1,
                    ValorCobrado = 150m,
                    ItensNecessarios = new[] { new { IdItemEstoque = idItemEmFalta, Quantidade = 1m } }
                }
            ]);
        await FinalizarDiagnosticoEAplicarOrcamentoAsync(ordemId);
        await AssertStatusAsync(ordemId, "AguardandoAprovacao");

        var token = await GetTokenAprovacaoAsync(ordemId);
        await AprovarPublicamenteAsync(token, DocumentoMariaOliveira);
        await AssertStatusAsync(ordemId, "ChecandoEstoque");

        await AplicarEstoqueEmFaltaAsync(ordemId, idItemEmFalta);
        await AssertStatusAsync(ordemId, "AguardandoPeca");

        // Assert
        await ExecutarAteEntregueAsync(ordemId);
    }
}
