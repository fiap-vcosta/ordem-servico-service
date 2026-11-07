using IntegrationTests.Infrastructure;

namespace IntegrationTests.OrdemServico;

[Collection(nameof(IntegrationFixture))]
public class ItensEmFaltaAteEntregueIntegrationTests(CustomWebApplicationFactory factory)
    : OrdemServicoIntegrationTestBase(factory)
{
    [Fact]
    public async Task Fluxo_AprovarInternamente_ItemEmFalta_Reabastecer_AteEntregue()
    {
        // Arrange
        AuthenticateAsStaff();
        var idItemSemSaldo = await CriarItemEstoqueSemSaldoAsync("EMF-INT");

        // Act
        var ordemId = await CriarOrdemAsync(
            idVeiculo: 5,
            servicos:
            [
                new
                {
                    IdServico = 1,
                    ValorCobrado = 150m,
                    ItensNecessarios = new[] { new { IdItemEstoque = idItemSemSaldo, Quantidade = 1m } }
                }
            ]);
        await FinalizarDiagnosticoEAplicarOrcamentoAsync(ordemId);
        await AprovarInternamenteAsync(ordemId);
        await AssertStatusAsync(ordemId, "AguardandoPeca");

        await RegistrarEntradaEstoqueAsync(idItemEstoque: idItemSemSaldo, quantidade: 5m);
        await AssertStatusAsync(ordemId, "LiberadaParaExecucao");

        // Assert
        await ExecutarAteEntregueAsync(ordemId);
    }

    [Fact]
    public async Task Fluxo_AprovarPublicamente_ItemEmFalta_Reabastecer_AteEntregue()
    {
        // Arrange
        AuthenticateAsStaff();
        var idItemSemSaldo = await CriarItemEstoqueSemSaldoAsync("EMF-PUB");

        // Act
        var ordemId = await CriarOrdemAsync(
            idVeiculo: 5,
            servicos:
            [
                new
                {
                    IdServico = 1,
                    ValorCobrado = 150m,
                    ItensNecessarios = new[] { new { IdItemEstoque = idItemSemSaldo, Quantidade = 1m } }
                }
            ]);
        await FinalizarDiagnosticoEAplicarOrcamentoAsync(ordemId);
        await AssertStatusAsync(ordemId, "AguardandoAprovacao");

        var token = await GetTokenAprovacaoAsync(ordemId);
        await AprovarPublicamenteAsync(token, DocumentoMariaOliveira);
        await AssertStatusAsync(ordemId, "AguardandoPeca");

        await RegistrarEntradaEstoqueAsync(idItemEstoque: idItemSemSaldo, quantidade: 5m);
        await AssertStatusAsync(ordemId, "LiberadaParaExecucao");

        // Assert
        await ExecutarAteEntregueAsync(ordemId);
    }
}
