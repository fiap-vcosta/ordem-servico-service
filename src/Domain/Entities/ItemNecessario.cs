using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

public enum StatusItemEstoque
{
    EstoqueNaoChecado,
    EstoqueEmFalta,
    EstoqueDisponivel,
    EstoqueTravado,
    Utilizado
}

public class ItemNecessario
{
    public int Id {  get; private set; }
    public int IdOrdemServico { get; private set; }
    public int IdItemOrdemServico { get; private set; }
    public StatusItemEstoque Status { get; private set; }
    public decimal Quantidade { get; init; }
    public required ItemEstoqueOrdemServico ItemEstoque { get; init; }
    
    public record CriarItemNecessarioParams(int IdOrdemServico, decimal Quantidade, ItemEstoqueOrdemServico ItemEstoque);
    public static ItemNecessario Criar(CriarItemNecessarioParams @params) => new()
    {
        IdOrdemServico = @params.IdOrdemServico,
        Status = StatusItemEstoque.EstoqueNaoChecado,
        Quantidade = @params.Quantidade,
        ItemEstoque = @params.ItemEstoque
    };

    public void AplicarStatusEstoque(StatusItemEstoque status)
    {
        if (Status is not (
            StatusItemEstoque.EstoqueNaoChecado
            or StatusItemEstoque.EstoqueEmFalta
            or StatusItemEstoque.EstoqueDisponivel
            or StatusItemEstoque.EstoqueTravado))
        {
            throw new BusinessRuleException($"Item de estoque {Id} com status {Status} não pode receber status de estoque.");
        }

        if (status is not (
            StatusItemEstoque.EstoqueEmFalta
            or StatusItemEstoque.EstoqueDisponivel
            or StatusItemEstoque.EstoqueTravado))
        {
            throw new BusinessRuleException($"Status de estoque {status} inválido para item {Id}.");
        }

        Status = status;
    }

    public void ConfirmarUtilizacao()
    {
        if (Status is not StatusItemEstoque.EstoqueTravado)
        {
            throw new BusinessRuleException($"Item de estoque {Id} com status {Status} não pode ser utilizado.");
        }

        Status = StatusItemEstoque.Utilizado;
    }
}