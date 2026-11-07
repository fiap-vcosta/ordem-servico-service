using Api.Presenters.OrdemServico;
using Api.ViewModels.OrdemServico;
using Application.UseCases;
using Application.UseCases.Commands.CriarOrdemServico;
using Application.UseCases.Responses;
using Domain.Entities;

namespace UnitTests.Api.Presenters.OrdemServico;

public class OrdemServicoPresenterTests
{
    private readonly OrdemServicoPresenter _presenter = new();

    [Fact]
    public void Present_MapsOrdemServicoResponseToViewModel()
    {
        // Arrange
        var response = new OrdemServicoResponse
        {
            Id = 3,
            Status = StatusOrdemServico.AguardandoAprovacao,
            ValorTotal = 250m,
            RecebidaEm = DateTime.UtcNow,
            AprovadaEm = DateTime.UtcNow,
            Cliente = new ClienteOrdemServicoResponse { Id = 1, Nome = "Maria", Email = "maria@teste.com" },
            Veiculo = new VeiculoOrdemServicoResponse { Placa = "ABC-1234", Marca = "VW", Modelo = "Gol" },
            Servicos = [],
            ItensNecessariosParaExecucao = []
        };

        // Act
        var viewModel = _presenter.Present(response);

        // Assert
        Assert.IsType<OrdemServicoViewModel>(viewModel);
        Assert.Equal(response.Id, viewModel.Id);
        Assert.Equal(response.Status, viewModel.Status);
        Assert.Equal(response.ValorTotal, viewModel.ValorTotal);
        Assert.Equal(response.Cliente.Nome, viewModel.Cliente.Nome);
        Assert.Equal(response.Veiculo.Placa, viewModel.Veiculo.Placa);
    }

    [Fact]
    public void Present_MapsCriarOrdemServicoCommandResponseToViewModel()
    {
        // Arrange
        var response = new CriarOrdemServicoCommandResponse
        {
            Id = 5,
            Status = StatusOrdemServico.Recebida,
            ValorTotal = 0m,
            RecebidaEm = DateTime.UtcNow,
            Cliente = new ClienteOrdemServicoResponse { Id = 1, Nome = "João", Email = "joao@teste.com" },
            Veiculo = new VeiculoOrdemServicoResponse { Placa = "XYZ-9876", Marca = "Fiat", Modelo = "Uno" },
            Servicos = []
        };

        // Act
        var viewModel = _presenter.Present(response);

        // Assert
        Assert.IsType<CriarOrdemServicoViewModel>(viewModel);
        Assert.Equal(response.Id, viewModel.Id);
        Assert.Equal(response.Status, viewModel.Status);
        Assert.Equal(response.RecebidaEm, viewModel.RecebidaEm);
        Assert.Equal(response.Cliente.Email, viewModel.Cliente.Email);
        Assert.Equal(response.Veiculo.Modelo, viewModel.Veiculo.Modelo);
    }
}
