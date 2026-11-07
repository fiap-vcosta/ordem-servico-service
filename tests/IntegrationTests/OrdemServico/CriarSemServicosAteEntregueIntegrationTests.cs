using IntegrationTests.Infrastructure;

namespace IntegrationTests.OrdemServico;

[Collection(nameof(IntegrationFixture))]
public class CriarSemServicosAteEntregueIntegrationTests(CustomWebApplicationFactory factory)
    : OrdemServicoIntegrationTestBase(factory)
{
    [Fact]
    public async Task Fluxo_CriarSemServicos_AdicionarDepois_AprovarInternamente_AteEntregue()
    {
        // Arrange
        AuthenticateAsStaff();

        // Act
        var ordemId = await CriarOrdemAsync(idVeiculo: 1);
        await AssertStatusAsync(ordemId, "EmDiagnostico");
        Assert.Equal(0, await GetServicosCountAsync(ordemId));

        await AdicionarServicoAsync(ordemId, idServico: 1, valorCobrado: 150m, idItemEstoque: 1, quantidade: 1m);
        await FinalizarDiagnosticoEAplicarOrcamentoAsync(ordemId);
        await AssertStatusAsync(ordemId, "AguardandoAprovacao");

        await AprovarInternamenteAsync(ordemId);
        await AssertStatusAsync(ordemId, "ChecandoEstoque");

        // Assert
        await ExecutarAteEntregueAsync(ordemId);
    }

    [Fact]
    public async Task Fluxo_CriarComListaServicosVazia_AdicionarDepois_AteEntregue()
    {
        // Arrange
        AuthenticateAsStaff();

        // Act
        var ordemId = await CriarOrdemAsync(idVeiculo: 1, servicos: Array.Empty<object>());
        await AssertStatusAsync(ordemId, "EmDiagnostico");
        Assert.Equal(0, await GetServicosCountAsync(ordemId));

        await AdicionarServicoAsync(ordemId, idServico: 2, valorCobrado: 50m);
        await FinalizarDiagnosticoEAplicarOrcamentoAsync(ordemId);
        await AprovarInternamenteAsync(ordemId);
        await AssertStatusAsync(ordemId, "ChecandoEstoque");

        // Assert
        await ExecutarAteEntregueAsync(ordemId);
    }
}
