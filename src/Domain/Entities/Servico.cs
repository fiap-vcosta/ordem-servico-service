using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

public enum StatusItemOrdemServico
{
    Sugerido,
    Aprovado,
    Rejeitado,
    Concluido,
}

public class Servico
{
    public int Id {  get; private set; }
    public int IdOrdemServico { get; private set; }
    public StatusItemOrdemServico Status { get; private set; }
    
    public DateTime? AprovadoEm { get; private set; }
    public DateTime? RejeitadoEm { get; private set; }
    public DateTime? ExecucaoIniciadaEm { get; private set; }
    public DateTime? ExecucaoFinalizadaEm { get; private set; }
    
    public string Nome { get; private set; } = string.Empty;
    public decimal ValorCobrado { get; private set; }
    
    public required ServicoCatalogo ServicoCatalogo { get; init; }
    
    private readonly List<ItemNecessario> _itensNecessarios = new();
    public IReadOnlyCollection<ItemNecessario> ItensNecessarios => _itensNecessarios.AsReadOnly();

    public static Servico Criar(string nome, decimal valorCobrado, ServicoCatalogo servicoCatalogo)
    {
        return new Servico
        {
            ServicoCatalogo = servicoCatalogo,
            Status = StatusItemOrdemServico.Sugerido,
            Nome = nome,
            ValorCobrado = valorCobrado
        };
    }
    
    public void AdicionarItemNecessario(ItemNecessario.CriarItemNecessarioParams @params)
    {
        var itemEstoqueOrdemServico = ItemNecessario.Criar(@params);
        _itensNecessarios.Add(itemEstoqueOrdemServico);
    }

    public void Rejeitar()
    {
        if (Status is not StatusItemOrdemServico.Sugerido)
        {
            throw new BusinessRuleException($"Serviço {Id} com status {Status} não pode ser rejeitado.");
        }
        
        Status = StatusItemOrdemServico.Rejeitado;
        RejeitadoEm =  DateTime.UtcNow;
    }

    public void Aprovar()
    {
        if (Status is not StatusItemOrdemServico.Sugerido)
        {
            throw new BusinessRuleException($"Serviço {Id} com status {Status} não pode ser aprovado.");
        }
        
        Status = StatusItemOrdemServico.Aprovado;
        AprovadoEm =  DateTime.UtcNow;
    }

    public void ConfirmarConclusao(DateTime iniciadoEm, DateTime finalizadoEm)
    {
        if (Status is not StatusItemOrdemServico.Aprovado)
        {
            throw new BusinessRuleException($"Serviço {Id} com status {Status} não pode ser concluído.");
        }

        Status = StatusItemOrdemServico.Concluido;
        ExecucaoIniciadaEm = iniciadoEm;
        ExecucaoFinalizadaEm = finalizadoEm;
    }
}