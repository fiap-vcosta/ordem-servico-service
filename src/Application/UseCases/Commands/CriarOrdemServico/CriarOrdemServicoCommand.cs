using MediatR;

namespace Application.UseCases.Commands.CriarOrdemServico;

public class CriarOrdemServicoCommand : IRequest<CriarOrdemServicoCommandResponse>
{
    public class ItemNecessario
    {
        public int IdItemEstoque { get; init; }
        public decimal Quantidade { get; init; }
    }

    public class Servico
    {
        public int IdServico { get; init; }
        public decimal ValorCobrado { get; init; }
        public IList<ItemNecessario> ItensNecessarios { get; init; } = [];
    }

    public int IdVeiculo { get; init; }
    public IList<Servico> Servicos { get; init; } = [];
}
