using IntegrationTests.Infrastructure;

namespace IntegrationTests.OrdemServico;

[Collection(nameof(IntegrationFixture))]
public class CriarComServicosAteEntregueIntegrationTests(CustomWebApplicationFactory factory)
    : OrdemServicoIntegrationTestBase(factory)
{
    [Fact]
    public async Task Fluxo_CriarComServicos_AprovarInternamente_AteEntregue()
    {
        // Arrange
        AuthenticateAsStaff();

        // Act
        var ordemId = await CriarOrdemAsync(
            idVeiculo: 2,
            servicos:
            [
                new
                {
                    IdServico = 1,
                    ValorCobrado = 150m,
                    ItensNecessarios = new[] { new { IdItemEstoque = 1, Quantidade = 1m } }
                }
            ]);
        await AssertStatusAsync(ordemId, "EmDiagnostico");
        Assert.True(await GetServicosCountAsync(ordemId) >= 1);

        await FinalizarDiagnosticoEAplicarOrcamentoAsync(ordemId);
        await AssertStatusAsync(ordemId, "AguardandoAprovacao");

        await AprovarInternamenteAsync(ordemId);
        await AssertStatusAsync(ordemId, "LiberadaParaExecucao");

        // Assert
        await ExecutarAteEntregueAsync(ordemId);
    }

    [Fact]
    public async Task Fluxo_CriarComServicos_AprovarPublicamente_AteEntregue()
    {
        // Arrange
        AuthenticateAsStaff();

        // Act
        var ordemId = await CriarOrdemAsync(
            idVeiculo: 2,
            servicos:
            [
                new
                {
                    IdServico = 1,
                    ValorCobrado = 150m,
                    ItensNecessarios = new[] { new { IdItemEstoque = 1, Quantidade = 1m } }
                }
            ]);
        await AssertStatusAsync(ordemId, "EmDiagnostico");

        await FinalizarDiagnosticoEAplicarOrcamentoAsync(ordemId);
        await AssertStatusAsync(ordemId, "AguardandoAprovacao");

        var token = await GetTokenAprovacaoAsync(ordemId);
        await AprovarPublicamenteAsync(token);
        await AssertStatusAsync(ordemId, "LiberadaParaExecucao");

        // Assert
        await ExecutarAteEntregueAsync(ordemId);
    }
}
