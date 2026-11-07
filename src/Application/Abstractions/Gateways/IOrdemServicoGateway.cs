using Domain.Entities;

namespace Application.Abstractions.Gateways;

public interface IOrdemServicoGateway
{
    Task<OrdemServicoAggregateRoot?> GetByIdAsync(int idOrdemServico);
    Task<OrdemServicoAggregateRoot?> GetByTokenAsync(string tokenAprovacao);
    Task<OrdemServicoAggregateRoot?> GetByTokenEDocumentoAsync(string tokenAprovacao, string documentoCliente);
    Task<IReadOnlyList<OrdemServicoAggregateRoot>> ListarAtivasAsync();
    Task<IEnumerable<OrdemServicoAggregateRoot>> GetAguardandoPecaPorItemEstoqueAsync(int idItemEstoque);
    Task CriarAsync(OrdemServicoAggregateRoot ordemServico);
    Task UpdateAsync(OrdemServicoAggregateRoot ordemServico);
}
