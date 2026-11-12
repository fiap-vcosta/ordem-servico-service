using UnitTests.Support;
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
            Id = OrdemServicoTestIds.Of(3),
            Status = StatusOrdemServico.AguardandoAprovacao,
            ValorTotal = 250m,
            RecebidaEm = DateTime.UtcNow,
            AprovadaEm = DateTime.UtcNow,
            Cliente = new ClienteOrdemServicoResponse { Id = 1, Nome = "Maria", Email = "maria@teste.com" },
            Veiculo = new VeiculoOrdemServicoResponse { Placa = "ABC-1234", Marca = "VW", Modelo = "Gol" },
            Servicos =
            [
                new ServicoOrdemServicoResponse
                {
                    Id = 10,
                    IdOrdemServico = OrdemServicoTestIds.Of(3),
                    Status = StatusItemOrdemServico.Sugerido,
                    Nome = "Troca",
                    ValorCobrado = 150m,
                    ServicoCatalogo = new ServicoCatalogoResponse { Id = 1, Nome = "Troca", Codigo = "MTR-001" },
                    ItensNecessarios =
                    [
                        new ItemNecessarioResponse
                        {
                            Id = 20,
                            IdOrdemServico = OrdemServicoTestIds.Of(3),
                            IdItemOrdemServico = 10,
                            Status = StatusItemEstoque.EstoqueNaoChecado,
                            Quantidade = 2m,
                            ItemEstoque = new ItemEstoqueOrdemServicoResponse
                            {
                                Id = 100,
                                Codigo = "PEC",
                                Nome = "Peça",
                                UnidadeMedida = "Unidade"
                            }
                        }
                    ]
                }
            ],
            ItensNecessariosParaExecucao =
            [
                new ItemNecessarioResponse
                {
                    Id = 21,
                    IdOrdemServico = OrdemServicoTestIds.Of(3),
                    IdItemOrdemServico = 10,
                    Status = StatusItemEstoque.EstoqueDisponivel,
                    Quantidade = 1m,
                    ItemEstoque = new ItemEstoqueOrdemServicoResponse
                    {
                        Id = 101,
                        Codigo = "OLE",
                        Nome = "Óleo",
                        UnidadeMedida = "Litro"
                    }
                }
            ]
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
        var servico = Assert.Single(viewModel.Servicos);
        Assert.Equal("Troca", servico.Nome);
        Assert.Equal("MTR-001", servico.ServicoCatalogo.Codigo);
        var item = Assert.Single(servico.ItensNecessarios);
        Assert.Equal(100, item.ItemEstoque.Id);
        Assert.Equal("Peça", item.ItemEstoque.Nome);
        var execucao = Assert.Single(viewModel.ItensNecessariosParaExecucao);
        Assert.Equal(StatusItemEstoque.EstoqueDisponivel, execucao.Status);
        Assert.Equal("Óleo", execucao.ItemEstoque.Nome);
    }

    [Fact]
    public void Present_MapsCriarOrdemServicoCommandResponseToViewModel()
    {
        // Arrange
        var response = new CriarOrdemServicoCommandResponse
        {
            Id = OrdemServicoTestIds.Of(5),
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

    [Fact]
    public void Present_MapsTempoMedioExecucaoResponses()
    {
        // Arrange
        var responses = new[]
        {
            TempoMedioExecucaoResponse.From(7, 4, TimeSpan.FromMinutes(45))
        };

        // Act
        var viewModels = _presenter.Present(responses).ToList();

        // Assert
        var item = Assert.Single(viewModels);
        Assert.Equal(7, item.IdServico);
        Assert.Equal(4, item.TotalExecucoes);
        Assert.Equal(TimeSpan.FromMinutes(45), item.ExecucaoMedia);
    }

    [Fact]
    public void Present_MapsListOfOrdemServicoResponses()
    {
        // Arrange
        var responses = new List<OrdemServicoResponse>
        {
            new()
            {
                Id = OrdemServicoTestIds.Of(1),
                Status = StatusOrdemServico.Recebida,
                ValorTotal = 0m,
                RecebidaEm = DateTime.UtcNow,
                Cliente = new ClienteOrdemServicoResponse { Id = 1, Nome = "A", Email = "a@t.com" },
                Veiculo = new VeiculoOrdemServicoResponse { Placa = "AAA", Marca = "VW", Modelo = "Gol" },
                Servicos = [],
                ItensNecessariosParaExecucao = []
            }
        };

        // Act
        var viewModels = _presenter.Present(responses);

        // Assert
        Assert.Single(viewModels);
        Assert.Equal(OrdemServicoTestIds.Of(1), viewModels[0].Id);
    }
}
