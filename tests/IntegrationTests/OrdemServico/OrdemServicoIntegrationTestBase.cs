using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Application.UseCases.Commands.AplicarCobrancaGerada;
using Application.UseCases.Commands.AplicarOrcamentoGerado;
using Application.UseCases.Commands.AplicarPagamentoConfirmado;
using IntegrationTests.Infrastructure;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.OrdemServico;

public abstract class OrdemServicoIntegrationTestBase
{
    protected const string DocumentoJoaoSilva = "86421537090";
    protected const string DocumentoMariaOliveira = "71284693031";

    protected static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected OrdemServicoIntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected void AuthenticateAsStaff() =>
        AuthHelper.AuthenticateAsStaff(Client);

    protected void ClearAuthentication() =>
        Client.DefaultRequestHeaders.Authorization = null;

    protected void AuthenticateAsCliente(string documento) =>
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwt.CreateClienteToken(documento));


    protected async Task<int> CriarOrdemAsync(int idVeiculo, IEnumerable<object>? servicos = null)
    {
        object request = servicos is null
            ? new { IdVeiculo = idVeiculo }
            : new { IdVeiculo = idVeiculo, Servicos = servicos };

        var response = await Client.PostAsJsonAsync("/ordem-servico", request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<IdResponse>(JsonOptions);
        Assert.NotNull(body);
        return body.Id;
    }

    protected async Task AdicionarServicoAsync(int ordemId, int idServico, decimal valorCobrado, int? idItemEstoque = null, decimal quantidade = 0m)
    {
        object request = idItemEstoque is null
            ? new { IdServico = idServico, ValorCobrado = valorCobrado, ItensNecessarios = Array.Empty<object>() }
            : new
            {
                IdServico = idServico,
                ValorCobrado = valorCobrado,
                ItensNecessarios = new[] { new { IdItemEstoque = idItemEstoque.Value, Quantidade = quantidade } }
            };

        var response = await Client.PostAsJsonAsync($"/ordem-servico/{ordemId}/adicionar-servico", request);
        response.EnsureSuccessStatusCode();
    }

    protected async Task FinalizarDiagnosticoAsync(int ordemId)
    {
        var response = await Client.PostAsync($"/ordem-servico/{ordemId}/finalizar-diagnostico", null);
        response.EnsureSuccessStatusCode();
    }

    protected async Task AplicarOrcamentoGeradoAsync(int ordemId)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var versao = await db.OrdensServico
            .AsNoTracking()
            .Where(o => o.Id == ordemId)
            .Select(o => o.VersaoDiagnostico)
            .FirstAsync();

        await mediator.Send(new AplicarOrcamentoGeradoCommand
        {
            IdOrdemServico = ordemId,
            VersaoDiagnostico = versao
        });
    }

    protected async Task FinalizarDiagnosticoEAplicarOrcamentoAsync(int ordemId)
    {
        await FinalizarDiagnosticoAsync(ordemId);
        await AplicarOrcamentoGeradoAsync(ordemId);
    }

    protected async Task AprovarInternamenteAsync(int ordemId)
    {
        var response = await Client.PostAsync($"/ordem-servico/{ordemId}/aprovar", null);
        response.EnsureSuccessStatusCode();
    }

    protected async Task RejeitarInternamenteAsync(int ordemId)
    {
        var response = await Client.PostAsync($"/ordem-servico/{ordemId}/rejeitar", null);
        response.EnsureSuccessStatusCode();
    }

    protected async Task AprovarParcialmenteAsync(int ordemId, params int[] idsServicosAprovados)
    {
        var response = await Client.PostAsJsonAsync($"/ordem-servico/{ordemId}/aprovar-parcialmente", new
        {
            IdsServicosAprovados = idsServicosAprovados
        });
        response.EnsureSuccessStatusCode();
    }

    protected async Task AprovarPublicamenteAsync(string token, string documentoCliente = DocumentoJoaoSilva)
    {
        AuthenticateAsCliente(documentoCliente);
        var response = await Client.PostAsync($"/ordem-servico/aprovar?token={Uri.EscapeDataString(token)}", null);
        response.EnsureSuccessStatusCode();
        AuthenticateAsStaff();
    }

    protected async Task RejeitarPublicamenteAsync(string token, string documentoCliente = DocumentoJoaoSilva)
    {
        AuthenticateAsCliente(documentoCliente);
        var response = await Client.PostAsync($"/ordem-servico/rejeitar?token={Uri.EscapeDataString(token)}", null);
        response.EnsureSuccessStatusCode();
        AuthenticateAsStaff();
    }

    protected async Task DescartarAsync(int ordemId)
    {
        var response = await Client.PostAsync($"/ordem-servico/{ordemId}/descartar", null);
        response.EnsureSuccessStatusCode();
    }

    protected async Task ConfirmarExecucaoAsync(int ordemId, params int[] idsServicos)
    {
        var response = await Client.PostAsJsonAsync($"/ordem-servico/{ordemId}/confirmar-execucao", new
        {
            ServicosExecutados = idsServicos.Select(idServico => new
            {
                IdServico = idServico,
                IniciadoEm = DateTime.UtcNow.AddHours(-1),
                FinalizadoEm = DateTime.UtcNow
            }).ToArray()
        });
        response.EnsureSuccessStatusCode();
    }

    protected async Task AplicarCobrancaEPagamentoAsync(int ordemId)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new AplicarCobrancaGeradaCommand { IdOrdemServico = ordemId });
        await mediator.Send(new AplicarPagamentoConfirmadoCommand { IdOrdemServico = ordemId });
    }

    protected async Task RegistrarEntradaEstoqueAsync(int idItemEstoque, decimal quantidade)
    {
        var response = await Client.PostAsJsonAsync($"/ordem-servico/itens-estoque/{idItemEstoque}/registrar-entrada", new
        {
            Quantidade = quantidade
        });
        response.EnsureSuccessStatusCode();
    }

    protected async Task<int> CriarItemEstoqueSemSaldoAsync(string codigoPrefixo)
    {
        var codigo = $"{codigoPrefixo}-{Guid.NewGuid():N}"[..20];
        var response = await Client.PostAsJsonAsync("/ordem-servico/itens-estoque", new
        {
            Codigo = codigo,
            Tipo = "Peca",
            Nome = $"Peça sem saldo {codigoPrefixo}",
            UnidadeMedida = "Unidade",
            PrecoVenda = 10m,
            Saldo = 0m
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<IdResponse>(JsonOptions);
        Assert.NotNull(body);
        return body.Id;
    }

    protected async Task AssertStatusAsync(int ordemId, string expectedStatus)
    {
        var response = await Client.GetAsync($"/ordem-servico/{ordemId}");
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var status = document.RootElement.GetProperty("status").GetString();
        Assert.Equal(expectedStatus, status);
    }

    protected async Task AssertListagemContemOrdemComStatusAsync(int ordemId, string expectedStatus)
    {
        var response = await Client.GetAsync("/ordem-servico");
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var ordem = document.RootElement.EnumerateArray()
            .FirstOrDefault(e => e.GetProperty("id").GetInt32() == ordemId);
        Assert.Equal(JsonValueKind.Object, ordem.ValueKind);
        Assert.Equal(expectedStatus, ordem.GetProperty("status").GetString());
    }

    protected async Task<int> GetPrimeiroServicoIdAsync(int ordemId)
    {
        var ids = await GetServicoIdsAsync(ordemId);
        Assert.NotEmpty(ids);
        return ids[0];
    }

    protected async Task<int[]> GetServicoIdsAsync(int ordemId)
    {
        var response = await Client.GetAsync($"/ordem-servico/{ordemId}");
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement.GetProperty("servicos")
            .EnumerateArray()
            .Select(s => s.GetProperty("id").GetInt32())
            .ToArray();
    }

    protected async Task<int[]> GetServicoIdsPorStatusAsync(int ordemId, string status)
    {
        var response = await Client.GetAsync($"/ordem-servico/{ordemId}");
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement.GetProperty("servicos")
            .EnumerateArray()
            .Where(s => s.GetProperty("status").GetString() == status)
            .Select(s => s.GetProperty("id").GetInt32())
            .ToArray();
    }

    protected async Task<int> AdicionarServicoERetornarIdAsync(
        int ordemId,
        int idServico,
        decimal valorCobrado,
        int? idItemEstoque = null,
        decimal quantidade = 0m)
    {
        var idsAntes = (await GetServicoIdsAsync(ordemId)).ToHashSet();
        await AdicionarServicoAsync(ordemId, idServico, valorCobrado, idItemEstoque, quantidade);
        var idsDepois = await GetServicoIdsAsync(ordemId);
        return Assert.Single(idsDepois.Where(id => !idsAntes.Contains(id)));
    }

    protected async Task<int> GetServicosCountAsync(int ordemId)
    {
        var response = await Client.GetAsync($"/ordem-servico/{ordemId}");
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement.GetProperty("servicos").GetArrayLength();
    }

    protected async Task<string> GetTokenAprovacaoAsync(int ordemId)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var token = await db.OrdensServico
            .AsNoTracking()
            .Where(o => o.Id == ordemId)
            .Select(o => o.TokenAprovacao)
            .FirstAsync();
        Assert.False(string.IsNullOrWhiteSpace(token));
        return token;
    }

    protected async Task ExecutarAteEntregueAsync(int ordemId)
    {
        var aprovados = await GetServicoIdsPorStatusAsync(ordemId, "Aprovado");
        Assert.NotEmpty(aprovados);
        await ConfirmarExecucaoAsync(ordemId, aprovados);
        await AssertStatusAsync(ordemId, "Finalizada");
        await AplicarCobrancaEPagamentoAsync(ordemId);
        await AssertStatusAsync(ordemId, "Entregue");
    }

    protected sealed record IdResponse(int Id);
}
