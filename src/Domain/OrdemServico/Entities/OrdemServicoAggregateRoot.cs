using System.Security.Cryptography;
using Domain.Exceptions;
using Domain.OrdemServico.ValueObjects;

namespace Domain.OrdemServico.Entities;

public enum StatusOrdemServico
{
    Recebida,
    EmDiagnostico,
    AguardandoOrcamento,
    AguardandoAprovacao,
    ChecandoEstoque,
    AguardandoPeca,
    LiberadaParaExecucao,
    EmExecucao,
    Finalizada,
    AguardandoPagamento,
    Descartada,
    Entregue,
}

public class OrdemServicoAggregateRoot
{
    public int Id { get; private set; }

    public StatusOrdemServico Status { get; private set; }

    public int VersaoDiagnostico { get; private set; }

    public decimal ValorTotal =>
        _servicos
            .Where(ios => ios.Status is not StatusItemOrdemServico.Rejeitado)
            .Sum(ios => ios.ValorCobrado);

    public DateTime RecebidaEm { get; private set; }
    public DateTime? EntregueEm { get; private set; }
    public DateTime? DescartadaEm { get; private set; }
    public DateTime? AprovadaEm { get; private set; }
    public string TokenAprovacao { get; private set; } = string.Empty;

    public required ClienteOrdemServico Cliente { get; init; }
    public required VeiculoOrdemServico Veiculo { get; init; }

    private readonly List<Servico> _servicos = new();
    public IReadOnlyCollection<Servico> Servicos => _servicos.AsReadOnly();

    public IEnumerable<ItemNecessario> ItensNecessariosParaExecucao => _servicos
        .Where(s => s.Status is StatusItemOrdemServico.Aprovado or StatusItemOrdemServico.Sugerido)
        .SelectMany(s => s.ItensNecessarios);

    public static OrdemServicoAggregateRoot Criar(ClienteOrdemServico cliente, VeiculoOrdemServico veiculo)
    {
        return new OrdemServicoAggregateRoot
        {
            Cliente = cliente,
            Veiculo = veiculo,
            Status = StatusOrdemServico.Recebida,
            VersaoDiagnostico = 0,
            RecebidaEm = DateTime.UtcNow,
            EntregueEm = null,
            DescartadaEm = null,
            TokenAprovacao = GerarTokenAprovacao()
        };
    }

    private static string GerarTokenAprovacao()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public void EnviarParaDiagnostico()
    {
        if (Status is not StatusOrdemServico.Recebida)
        {
            throw new BusinessRuleException($"Ordem de Serviço {Id} com status {Status} não pode ser enviada para diagnóstico.");
        }

        Status = StatusOrdemServico.EmDiagnostico;
    }

    public void Descartar()
    {
        if (Status is not (
            StatusOrdemServico.Recebida
            or StatusOrdemServico.EmDiagnostico
            or StatusOrdemServico.AguardandoOrcamento
            or StatusOrdemServico.AguardandoAprovacao
            or StatusOrdemServico.ChecandoEstoque
            or StatusOrdemServico.AguardandoPeca
            or StatusOrdemServico.LiberadaParaExecucao))
        {
            throw new BusinessRuleException($"Ordem de Serviço {Id} com status {Status} não pode ser descartada.");
        }

        Status = StatusOrdemServico.Descartada;
        DescartadaEm = DateTime.UtcNow;
    }

    public void AdicionarItemServico(string nome, decimal valorCobrado, ServicoCatalogo servicoCatalogo,
        List<ItemNecessario.CriarItemNecessarioParams> itensNecessarios)
    {
        if (Status is not (StatusOrdemServico.Recebida or StatusOrdemServico.EmDiagnostico))
        {
            throw new BusinessRuleException($"Ordem de Serviço {Id} com status {Status} não pode ter itens adicionados.");
        }

        var itemOrdemServico = Servico.Criar(nome, valorCobrado, servicoCatalogo);
        foreach (var itemNecessario in itensNecessarios)
        {
            itemOrdemServico.AdicionarItemNecessario(itemNecessario);
        }

        _servicos.Add(itemOrdemServico);
    }

    public void FinalizarDiagnostico()
    {
        if (Status is not StatusOrdemServico.EmDiagnostico)
        {
            throw new BusinessRuleException($"Ordem de Serviço {Id} com status {Status} não pode ter diagnóstico finalizado.");
        }

        if (_servicos.Count == 0)
        {
            throw new BusinessRuleException($"Ordem de Serviço {Id} não teve nenhum serviço adicionado.");
        }

        VersaoDiagnostico++;
        Status = StatusOrdemServico.AguardandoOrcamento;
    }

    public void AplicarOrcamentoGerado(int versaoDiagnostico)
    {
        if (Status is not StatusOrdemServico.AguardandoOrcamento)
        {
            throw new BusinessRuleException($"Ordem de Serviço {Id} com status {Status} não pode aplicar orçamento gerado.");
        }

        if (versaoDiagnostico != VersaoDiagnostico)
        {
            throw new BusinessRuleException(
                $"Ordem de Serviço {Id} esperava versão de diagnóstico {VersaoDiagnostico}, recebeu {versaoDiagnostico}.");
        }

        if (_servicos.Any(ios => ios.Status is StatusItemOrdemServico.Sugerido))
        {
            Status = StatusOrdemServico.AguardandoAprovacao;
            return;
        }

        if (_servicos.Any(ios => ios.Status is StatusItemOrdemServico.Aprovado))
        {
            EnviarParaChecagemEstoque();
            return;
        }

        Status = StatusOrdemServico.Entregue;
        EntregueEm = DateTime.UtcNow;
    }

    public void AplicarOrcamentoFalhou(int versaoDiagnostico)
    {
        if (Status is not StatusOrdemServico.AguardandoOrcamento)
        {
            throw new BusinessRuleException($"Ordem de Serviço {Id} com status {Status} não pode aplicar falha de orçamento.");
        }

        if (versaoDiagnostico != VersaoDiagnostico)
        {
            throw new BusinessRuleException(
                $"Ordem de Serviço {Id} esperava versão de diagnóstico {VersaoDiagnostico}, recebeu {versaoDiagnostico}.");
        }

        Status = StatusOrdemServico.EmDiagnostico;
    }

    public void RegistrarCobrancaGerada()
    {
        if (Status is StatusOrdemServico.AguardandoPagamento)
        {
            return;
        }

        if (Status is not StatusOrdemServico.Finalizada)
        {
            throw new BusinessRuleException($"Ordem de Serviço {Id} com status {Status} não pode registrar cobrança gerada.");
        }

        Status = StatusOrdemServico.AguardandoPagamento;
    }

    public void ConfirmarPagamentoRecebido()
    {
        if (Status is not StatusOrdemServico.AguardandoPagamento)
        {
            throw new BusinessRuleException($"Ordem de Serviço {Id} com status {Status} não pode confirmar pagamento.");
        }

        Status = StatusOrdemServico.Entregue;
        EntregueEm = DateTime.UtcNow;
    }

    public void RegistrarPagamentoRecusado()
    {
        if (Status is not StatusOrdemServico.AguardandoPagamento)
        {
            throw new BusinessRuleException($"Ordem de Serviço {Id} com status {Status} não pode registrar pagamento recusado.");
        }
    }

    public void RejeitarServicosSugeridos()
    {
        if (Status is not StatusOrdemServico.AguardandoAprovacao)
        {
            throw new BusinessRuleException($"Ordem de Serviço {Id} com status {Status} não pode ter serviços rejeitados.");
        }

        Status = StatusOrdemServico.EmDiagnostico;

        foreach (var servico in _servicos.Where(s => s.Status is StatusItemOrdemServico.Sugerido))
        {
            servico.Rejeitar();
        }
    }

    public void AprovarServicosSugeridos()
    {
        if (Status is not StatusOrdemServico.AguardandoAprovacao)
        {
            throw new BusinessRuleException($"Ordem de Serviço {Id} com status {Status} não pode ter serviços aprovados ou rejeitados.");
        }

        foreach (var servico in _servicos.Where(s => s.Status is StatusItemOrdemServico.Sugerido))
        {
            servico.Aprovar();
        }

        EnviarParaChecagemEstoque();
    }

    public void AprovarServicosParcialmente(List<int> idsItensServicoAprovados)
    {
        if (Status is not StatusOrdemServico.AguardandoAprovacao)
        {
            throw new BusinessRuleException($"Ordem de Serviço {Id} com status {Status} não pode ter serviços aprovados.");
        }

        foreach (var idItemServicoAprovado in idsItensServicoAprovados)
        {
            _servicos
                .First(ios => ios.Id == idItemServicoAprovado)
                .Aprovar();
        }

        if (_servicos.Any(ios => ios.Status is StatusItemOrdemServico.Sugerido))
        {
            RejeitarServicosSugeridos();
            return;
        }

        EnviarParaChecagemEstoque();
    }

    private void EnviarParaChecagemEstoque()
    {
        Status = StatusOrdemServico.ChecandoEstoque;
        AprovadaEm = DateTime.UtcNow;
    }

    public void ChecarItensNecessarios(Dictionary<int, decimal> saldosDisponiveis)
    {
        if (Status is not (StatusOrdemServico.ChecandoEstoque or StatusOrdemServico.AguardandoPeca))
        {
            throw new BusinessRuleException($"Ordem de Serviço {Id} com status {Status} não pode ter estoque checado.");
        }

        var clonedQuantidadesDisponiveis = new Dictionary<int, decimal>(saldosDisponiveis);

        foreach (var item in ItensNecessariosParaExecucao)
        {
            var saldoAtual = clonedQuantidadesDisponiveis[item.ItemEstoque.Id];

            item.ChecarEstoque(saldoAtual);

            if (item.Status == StatusItemEstoque.EstoqueDisponivel)
            {
                clonedQuantidadesDisponiveis[item.ItemEstoque.Id] = saldoAtual - item.Quantidade;
            }
        }

        Status = ItensNecessariosParaExecucao.All(item => item.Status == StatusItemEstoque.EstoqueDisponivel)
            ? StatusOrdemServico.LiberadaParaExecucao
            : StatusOrdemServico.AguardandoPeca;
    }

    public void TravarItensNecessarios()
    {
        foreach (var item in ItensNecessariosParaExecucao)
        {
            item.TravarEstoque();
        }
    }

    public void ConfirmarExecucao(List<ServicoExecutado> servicosExecutados)
    {
        if (Status is not (StatusOrdemServico.LiberadaParaExecucao or StatusOrdemServico.EmExecucao))
        {
            throw new BusinessRuleException($"Ordem de Serviço {Id} com status {Status} não pode ter execução confirmada.");
        }

        foreach (var servicoExecutado in servicosExecutados)
        {
            var servico = _servicos.First(s => s.Id == servicoExecutado.IdServico);
            servico.ConfirmarConclusao(servicoExecutado.IniciadoEm, servicoExecutado.FinalizadoEm);
        }

        if (_servicos.All(servico => servico.Status is (StatusItemOrdemServico.Concluido or StatusItemOrdemServico.Rejeitado)))
        {
            Status = StatusOrdemServico.Finalizada;
        }
        else
        {
            Status = StatusOrdemServico.EmExecucao;
        }
    }
}
