using Application.Abstractions.Gateways;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Gateways;

public class OrdemServicoGateway(AppDbContext context) : IOrdemServicoGateway
{
    private static readonly StatusOrdemServico[] StatusExcluidosDaListagem =
    [
        StatusOrdemServico.Finalizada,
        StatusOrdemServico.AguardandoPagamento,
        StatusOrdemServico.CobrancaExpirada,
        StatusOrdemServico.Entregue,
        StatusOrdemServico.Descartada
    ];

    public async Task CriarAsync(OrdemServicoAggregateRoot ordemServico)
    {
        context.OrdensServico.Add(ordemServico);
        await context.SaveChangesAsync();
    }

    public async Task<OrdemServicoAggregateRoot?> GetByIdAsync(Guid idOrdemServico)
    {
        return await QueryCompleta()
            .FirstOrDefaultAsync(os => os.Id == idOrdemServico);
    }

    public async Task<OrdemServicoAggregateRoot?> GetByTokenAsync(string tokenAprovacao)
    {
        return await QueryCompleta()
            .FirstOrDefaultAsync(os => os.TokenAprovacao == tokenAprovacao);
    }

    public async Task<OrdemServicoAggregateRoot?> GetByTokenEDocumentoAsync(
        string tokenAprovacao,
        string documentoCliente)
    {
        var documentoNormalizado = DocumentoNormalizer.Normalize(documentoCliente);

        return await QueryCompleta()
            .Where(os => os.TokenAprovacao == tokenAprovacao && os.Cliente.Documento == documentoNormalizado)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<OrdemServicoAggregateRoot>> ListarAtivasAsync()
    {
        var ordens = await QueryCompleta()
            .Where(os => !StatusExcluidosDaListagem.Contains(os.Status))
            .ToListAsync();

        return ordens
            .OrderByDescending(os => os.Status)
            .ThenBy(os => os.RecebidaEm)
            .ToList();
    }

    public async Task UpdateAsync(OrdemServicoAggregateRoot ordemServico)
    {
        context.OrdensServico.Update(ordemServico);
        await context.SaveChangesAsync();
    }

    private IQueryable<OrdemServicoAggregateRoot> QueryCompleta()
    {
        return context.OrdensServico
            .Include(os => os.Servicos)
            .ThenInclude(ios => ios.ItensNecessarios);
    }
}
