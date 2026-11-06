using Api.ViewModels.OrdemServico;
using Application.UseCases;
using Application.UseCases.Commands.AdicionarItemOrdemServico;
using Application.UseCases.Commands.AprovarOrdemServico;
using Application.UseCases.Commands.AprovarServicosParcialmente;
using Application.UseCases.Commands.CriarOrdemServico;
using Application.UseCases.Commands.DescartarOrdemServico;
using Application.UseCases.Commands.FinalizarDiagnostico;
using Application.UseCases.Commands.RejeitarOrdemServico;
using Application.UseCases.Responses;

namespace Api.Presenters.OrdemServico;

public class OrdemServicoPresenter
{
    public OrdemServicoViewModel Present(OrdemServicoResponse response)
    {
        return new OrdemServicoViewModel
        {
            Id = response.Id,
            Status = response.Status,
            ValorTotal = response.ValorTotal,
            RecebidaEm = response.RecebidaEm,
            EntregueEm = response.EntregueEm,
            DescartadaEm = response.DescartadaEm,
            AprovadaEm = response.AprovadaEm,
            Cliente = ClienteOrdemServicoViewModel.From(response.Cliente),
            Veiculo = VeiculoOrdemServicoViewModel.From(response.Veiculo),
            Servicos = ServicoOrdemServicoViewModel.FromMany(response.Servicos),
            ItensNecessariosParaExecucao = ItemNecessarioViewModel.FromMany(response.ItensNecessariosParaExecucao)
        };
    }

    public IReadOnlyList<OrdemServicoViewModel> Present(IReadOnlyList<OrdemServicoResponse> responses)
    {
        return responses.Select(Present).ToList();
    }

    public CriarOrdemServicoViewModel Present(CriarOrdemServicoCommandResponse response)
    {
        return new CriarOrdemServicoViewModel
        {
            Id = response.Id,
            Status = response.Status,
            ValorTotal = response.ValorTotal,
            RecebidaEm = response.RecebidaEm,
            Cliente = ClienteOrdemServicoViewModel.From(response.Cliente),
            Veiculo = VeiculoOrdemServicoViewModel.From(response.Veiculo),
            Servicos = ServicoOrdemServicoViewModel.FromMany(response.Servicos)
        };
    }

    public DescartarOrdemServicoViewModel Present(DescartarOrdemServicoResponse response)
    {
        return new DescartarOrdemServicoViewModel
        {
            Id = response.Id,
            Status = response.Status,
            RecebidaEm = response.RecebidaEm,
            DescartadaEm = response.DescartadaEm,
            Itens = ServicoOrdemServicoViewModel.FromMany(response.Itens),
            Cliente = ClienteOrdemServicoViewModel.From(response.Cliente),
            Veiculo = VeiculoOrdemServicoViewModel.From(response.Veiculo)
        };
    }

    public AdicionarItemOrdemServicoViewModel Present(AdicionarItemOrdemServicoCommandResponse response)
    {
        return new AdicionarItemOrdemServicoViewModel
        {
            Id = response.Id,
            Status = response.Status,
            ValorTotal = response.ValorTotal,
            RecebidaEm = response.RecebidaEm,
            Cliente = ClienteOrdemServicoViewModel.From(response.Cliente),
            Veiculo = VeiculoOrdemServicoViewModel.From(response.Veiculo),
            Itens = ServicoOrdemServicoViewModel.FromMany(response.Itens)
        };
    }

    public FinalizarDiagnosticoViewModel Present(FinalizarDiagnosticoCommandResponse response)
    {
        return new FinalizarDiagnosticoViewModel
        {
            Id = response.Id,
            Status = response.Status,
            ValorTotal = response.ValorTotal,
            RecebidaEm = response.RecebidaEm,
            Cliente = ClienteOrdemServicoViewModel.From(response.Cliente),
            Veiculo = VeiculoOrdemServicoViewModel.From(response.Veiculo),
            Servicos = ServicoOrdemServicoViewModel.FromMany(response.Servicos)
        };
    }

    public AprovarOrdemServicoViewModel Present(AprovarOrdemServicoCommandResponse response)
    {
        return new AprovarOrdemServicoViewModel
        {
            Id = response.Id,
            Status = response.Status,
            ValorTotal = response.ValorTotal,
            RecebidaEm = response.RecebidaEm,
            AprovadaEm = response.AprovadaEm,
            Cliente = ClienteOrdemServicoViewModel.From(response.Cliente),
            Veiculo = VeiculoOrdemServicoViewModel.From(response.Veiculo),
            Servicos = ServicoOrdemServicoViewModel.FromMany(response.Servicos)
        };
    }

    public RejeitarOrdemServicoViewModel Present(RejeitarOrdemServicoCommandResponse response)
    {
        return new RejeitarOrdemServicoViewModel
        {
            Id = response.Id,
            Status = response.Status,
            ValorTotal = response.ValorTotal,
            RecebidaEm = response.RecebidaEm,
            EntregueEm = response.EntregueEm,
            Cliente = ClienteOrdemServicoViewModel.From(response.Cliente),
            Veiculo = VeiculoOrdemServicoViewModel.From(response.Veiculo),
            Servicos = ServicoOrdemServicoViewModel.FromMany(response.Servicos)
        };
    }

    public AprovarServicosParcialmenteViewModel Present(AprovarServicosParcialmenteCommandResponse response)
    {
        return new AprovarServicosParcialmenteViewModel
        {
            Id = response.Id,
            Status = response.Status,
            ValorTotal = response.ValorTotal,
            RecebidaEm = response.RecebidaEm,
            Cliente = ClienteOrdemServicoViewModel.From(response.Cliente),
            Veiculo = VeiculoOrdemServicoViewModel.From(response.Veiculo),
            Servicos = ServicoOrdemServicoViewModel.FromMany(response.Servicos)
        };
    }

    public IEnumerable<TempoMedioExecucaoViewModel> Present(IEnumerable<TempoMedioExecucaoResponse> responses)
    {
        return responses.Select(r => new TempoMedioExecucaoViewModel
        {
            IdServico = r.IdServico,
            TotalExecucoes = r.TotalExecucoes,
            ExecucaoMedia = r.ExecucaoMedia
        });
    }
}
