namespace Api.Controllers.Staff.OrdemServico.AdicionarItemServico;

public class AdicionarItemServicoRequest
{
    public class ItemNecessario
    {
        public int IdItemEstoque { get; init; }
        public decimal Quantidade { get; init; }
    }
    
    public int IdServico { get; init; }
    public decimal ValorCobrado { get; init; }
    public IList<ItemNecessario> ItensNecessarios { get; init; } = [];
}