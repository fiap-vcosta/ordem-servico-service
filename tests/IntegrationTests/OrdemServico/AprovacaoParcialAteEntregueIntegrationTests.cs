using IntegrationTests.Infrastructure;

namespace IntegrationTests.OrdemServico;

[Collection(nameof(IntegrationFixture))]
public class AprovacaoParcialAteEntregueIntegrationTests(CustomWebApplicationFactory factory)
    : OrdemServicoIntegrationTestBase(factory)
{
    [Fact]
    public async Task Fluxo_AprovarParcialmente_DepoisFinalizarAprovados_AteEntregue()
    {
        // Arrange
        AuthenticateAsStaff();

        // Act
        var ordemId = await CriarOrdemAsync(
            idVeiculo: 4,
            servicos:
            [
                new
                {
                    IdServico = 1,
                    ValorCobrado = 100m,
                    ItensNecessarios = Array.Empty<object>()
                },
                new
                {
                    IdServico = 2,
                    ValorCobrado = 50m,
                    ItensNecessarios = Array.Empty<object>()
                }
            ]);
        await AssertStatusAsync(ordemId, "EmDiagnostico");
        Assert.Equal(2, await GetServicosCountAsync(ordemId));

        await FinalizarDiagnosticoEAplicarOrcamentoAsync(ordemId);
        await AssertStatusAsync(ordemId, "AguardandoAprovacao");

        var idsServicos = await GetServicoIdsAsync(ordemId);
        await AprovarParcialmenteAsync(ordemId, idsServicos[0]);
        await AssertStatusAsync(ordemId, "EmDiagnostico");

        await FinalizarDiagnosticoEAplicarOrcamentoAsync(ordemId);
        await AssertStatusAsync(ordemId, "ChecandoEstoque");

        // Assert
        await ExecutarAteEntregueAsync(ordemId);
    }
}
