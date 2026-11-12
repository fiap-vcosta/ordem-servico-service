namespace Api.Controllers.Staff.OrdemServico.AdicionarItemServico;

public class AdicionarItemServicoRequest
{
    public class ItemNecessario
    {
        public int IdItemEstoque { get; init; }
        public decimal Quantidade { get; init; }
    }
    
    public required int IdServico { get; init; }
    public required decimal ValorCobrado { get; init; }
    public IList<ItemNecessario> ItensNecessarios { get; init; } = [];
}