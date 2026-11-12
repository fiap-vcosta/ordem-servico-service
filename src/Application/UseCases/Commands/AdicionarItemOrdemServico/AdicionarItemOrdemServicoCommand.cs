using MediatR;

namespace Application.UseCases.Commands.AdicionarItemOrdemServico;

public class AdicionarItemOrdemServicoCommand : IRequest<AdicionarItemOrdemServicoCommandResponse>
{
    public class ItemNecessario
    {
        public int IdItemEstoque { get; init; }
        public decimal Quantidade { get; init; }
    }
    
    public Guid IdOrdemServico { get; init; }
    public int IdServico { get; init; }
    public decimal ValorCobrado { get; init; }
    public IList<ItemNecessario> ItensNecessarios { get; init; } = [];
}