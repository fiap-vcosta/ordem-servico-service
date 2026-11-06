using IntegrationTests.Infrastructure;

namespace IntegrationTests.OrdemServico;

[Collection(nameof(IntegrationFixture))]
public class RejeicaoAteDescartadaIntegrationTests(CustomWebApplicationFactory factory)
    : OrdemServicoIntegrationTestBase(factory)
{
    [Fact]
    public async Task Fluxo_RejeicaoInterna_DepoisDescartar_AteDescartada()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var ordemId = await CriarOrdemAsync(
            idVeiculo: 3,
            servicos:
            [
                new
                {
                    IdServico = 2,
                    ValorCobrado = 50m,
                    ItensNecessarios = Array.Empty<object>()
                }
            ]);
        await AssertStatusAsync(ordemId, "EmDiagnostico");

        await FinalizarDiagnosticoEAplicarOrcamentoAsync(ordemId);
        await AssertStatusAsync(ordemId, "AguardandoAprovacao");

        await RejeitarInternamenteAsync(ordemId);
        await AssertStatusAsync(ordemId, "EmDiagnostico");

        await DescartarAsync(ordemId);

        // Assert
        await AssertStatusAsync(ordemId, "Descartada");
    }

    [Fact]
    public async Task Fluxo_RejeicaoPublica_DepoisDescartar_AteDescartada()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var ordemId = await CriarOrdemAsync(
            idVeiculo: 3,
            servicos:
            [
                new
                {
                    IdServico = 2,
                    ValorCobrado = 50m,
                    ItensNecessarios = Array.Empty<object>()
                }
            ]);
        await FinalizarDiagnosticoEAplicarOrcamentoAsync(ordemId);
        await AssertStatusAsync(ordemId, "AguardandoAprovacao");

        var token = await GetTokenAprovacaoAsync(ordemId);
        await RejeitarPublicamenteAsync(token);
        await AssertStatusAsync(ordemId, "EmDiagnostico");

        await DescartarAsync(ordemId);

        // Assert
        await AssertStatusAsync(ordemId, "Descartada");
    }

    [Fact]
    public async Task Fluxo_RejeicaoInterna_FinalizarSemServicosPendentes_AteEntregue()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var ordemId = await CriarOrdemAsync(idVeiculo: 4);
        await AdicionarServicoAsync(ordemId, idServico: 2, valorCobrado: 50m);
        await FinalizarDiagnosticoEAplicarOrcamentoAsync(ordemId);
        await RejeitarInternamenteAsync(ordemId);
        await AssertStatusAsync(ordemId, "EmDiagnostico");

        await FinalizarDiagnosticoAsync(ordemId);
        await AssertStatusAsync(ordemId, "AguardandoOrcamento");
        await AplicarOrcamentoGeradoAsync(ordemId);

        // Assert
        await AssertStatusAsync(ordemId, "Entregue");
    }
}
