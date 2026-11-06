using Domain.Estoque.Entities;
using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Database;
using Infrastructure.Database.Gateways;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Infrastructure.Database.Gateways;

public class ItemEstoqueGatewayTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ItemEstoqueGateway _gateway;

    public ItemEstoqueGatewayTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated(); 
        
        _gateway = new ItemEstoqueGateway(_context);

        _context.ItensEstoque.AddRange(
            new ItemEstoqueAggregateRoot { Codigo = "ITM-001", Tipo = ItemTipo.Peca, Nome = "Filtro de óleo", UnidadeMedida = UnidadeMedida.Unidade, PrecoVenda = 45.5m, Saldo = 10m, SaldoReservado = 2m },
            new ItemEstoqueAggregateRoot { Codigo = "ITM-002", Tipo = ItemTipo.Insumo, Nome = "Óleo", UnidadeMedida = UnidadeMedida.Litro, PrecoVenda = 25.0m, Saldo = 20m, SaldoReservado = 3m }
        );
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsItemEstoque_WhenItemExists()
    {
        // Arrange
        var existingId = 1;

        // Act
        var item = await _gateway.GetByIdAsync(existingId);

        // Assert
        Assert.NotNull(item);
        Assert.Equal("INS-001", item.Codigo);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenItemDoesNotExist()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var item = await _gateway.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(item);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllItemsEstoque()
    {
        // Arrange

        // Act
        var items = (await _gateway.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(32, items.Count);
    }

    [Fact]
    public async Task GetByCodigoAsync_ReturnsItemEstoque_WhenCodigoExists()
    {
        // Arrange
        var existingCodigo = "ITM-002";

        // Act
        var item = await _gateway.GetByCodigoAsync(existingCodigo);

        // Assert
        Assert.NotNull(item);
        Assert.Equal(ItemTipo.Insumo, item.Tipo);
    }

    [Fact]
    public async Task GetEBloquearItensAsync_ReturnsEmptyList_WhenIdsAreEmpty()
    {
        // Arrange
        var emptyIds = new List<int>();

        // Act
        var itens = (await _gateway.GetEBloquearItensAsync(emptyIds)).ToList();

        // Assert
        Assert.Empty(itens);
    }

    [Fact]
    public async Task CreateAsync_AddsItemEstoque_ToDatabase()
    {
        // Arrange
        var newItem = new ItemEstoqueAggregateRoot { Codigo = "ITM-003", Tipo = ItemTipo.Peca, Nome = "Bateria", UnidadeMedida = UnidadeMedida.Unidade, PrecoVenda = 150m, Saldo = 5m, SaldoReservado = 0m };

        // Act
        await _gateway.CreateAsync(newItem);

        // Assert
        var created = await _gateway.GetByCodigoAsync("ITM-003");
        Assert.NotNull(created);
        Assert.Equal("Bateria", created.Nome);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesItemEstoque()
    {
        // Arrange
        var item = await _gateway.GetByIdAsync(1);
        Assert.NotNull(item);
        item.Nome = "Filtro de óleo premium";

        // Act
        await _gateway.UpdateAsync(item);

        // Assert
        var updated = await _gateway.GetByIdAsync(1);
        Assert.NotNull(updated);
        Assert.Equal("Filtro de óleo premium", updated.Nome);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItemEstoque()
    {
        // Arrange
        var idToDelete = 1;

        // Act
        await _gateway.DeleteAsync(idToDelete);

        // Assert
        var deleted = await _gateway.GetByIdAsync(1);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task GetUtilizadosByOrdemServico_ReturnsEstoqueItems_WhenOrdemServicoContainsItemEstoque()
    {
        // Arrange
        var cliente = new ClienteOrdemServico { Id = 1, Nome = "Cliente Teste", Email = "teste@teste.com" };
        var veiculo = new VeiculoOrdemServico { Placa = "ABC-1234", Marca = "Honda", Modelo = "Civic" };
        var servicoCatalogo = new ServicoCatalogo() { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        var ordem = OrdemServicoAggregateRoot.Criar(cliente, veiculo);
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico("Troca de óleo", 100m, servicoCatalogo, new List<ItemNecessario.CriarItemNecessarioParams>
        {
            new(1, 1m, new ItemEstoqueOrdemServico { Id = 1, Codigo = "ITM-001", Nome = "Filtro de óleo", UnidadeMedida = "Unidade" })
        });
        _context.OrdensServico.Add(ordem);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _gateway.GetUtilizadosByOrdemServico(ordem.Id)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("INS-001", result[0].Codigo);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _context.Dispose();
    }
}
