using IntegrationTests.Infrastructure;

namespace IntegrationTests.OrdemServico;

[Collection(nameof(IntegrationFixture))]
public class AprovacaoParcialComItensEmFaltaAteEntregueIntegrationTests(CustomWebApplicationFactory factory)
    : OrdemServicoIntegrationTestBase(factory)
{
    [Fact]
    public async Task Fluxo_Parcial_DepoisMaisServicosComItensEmFalta_Reabastecer_AteEntregue()
    {
        // Arrange
        AuthenticateAsStaff();

        // Act
        var ordemId = await CriarOrdemAsync(idVeiculo: 1);
        await AssertStatusAsync(ordemId, "EmDiagnostico");

        var idServicoInicialAprovado = await AdicionarServicoERetornarIdAsync(ordemId, idServico: 1, valorCobrado: 100m);
        await AdicionarServicoERetornarIdAsync(ordemId, idServico: 2, valorCobrado: 50m);
        await FinalizarDiagnosticoEAplicarOrcamentoAsync(ordemId);
        await AssertStatusAsync(ordemId, "AguardandoAprovacao");

        await AprovarParcialmenteAsync(ordemId, idServicoInicialAprovado);
        await AssertStatusAsync(ordemId, "EmDiagnostico");

        var idItemEmFalta1 = await CriarItemEstoqueSemSaldoAsync("PARC-1");
        var idItemEmFalta2 = await CriarItemEstoqueSemSaldoAsync("PARC-2");

        var idServicoComFalta1 = await AdicionarServicoERetornarIdAsync(
            ordemId, idServico: 1, valorCobrado: 120m, idItemEstoque: idItemEmFalta1, quantidade: 1m);
        var idServicoComFalta2 = await AdicionarServicoERetornarIdAsync(
            ordemId, idServico: 2, valorCobrado: 80m, idItemEstoque: idItemEmFalta2, quantidade: 2m);
        var idServicoParaRejeitar = await AdicionarServicoERetornarIdAsync(ordemId, idServico: 1, valorCobrado: 40m);

        await FinalizarDiagnosticoEAplicarOrcamentoAsync(ordemId);
        await AssertStatusAsync(ordemId, "AguardandoAprovacao");

        await AprovarParcialmenteAsync(ordemId, idServicoComFalta1, idServicoComFalta2);
        await AssertStatusAsync(ordemId, "EmDiagnostico");
        Assert.Contains(idServicoComFalta1, await GetServicoIdsPorStatusAsync(ordemId, "Aprovado"));
        Assert.Contains(idServicoComFalta2, await GetServicoIdsPorStatusAsync(ordemId, "Aprovado"));
        Assert.Contains(idServicoParaRejeitar, await GetServicoIdsPorStatusAsync(ordemId, "Rejeitado"));

        await FinalizarDiagnosticoEAplicarOrcamentoAsync(ordemId);
        await AssertStatusAsync(ordemId, "AguardandoPeca");
        await AssertListagemContemOrdemComStatusAsync(ordemId, "AguardandoPeca");

        await RegistrarEntradaEstoqueAsync(idItemEmFalta1, quantidade: 5m);
        await RegistrarEntradaEstoqueAsync(idItemEmFalta2, quantidade: 5m);
        await AssertStatusAsync(ordemId, "LiberadaParaExecucao");

        // Assert
        await ExecutarAteEntregueAsync(ordemId);
    }
}
