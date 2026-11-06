using Domain.Entities;
using Domain.Estoque.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ItemEstoqueAggregateRoot> ItensEstoque { get; set; }
    public DbSet<OrdemServicoAggregateRoot> OrdensServico { get; set; }
    public DbSet<Servico> ItensServicos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var itensEstoque = modelBuilder.Entity<ItemEstoqueAggregateRoot>();
        itensEstoque.HasKey(ie => ie.Id);
        itensEstoque.HasIndex(ie => ie.Codigo).IsUnique();
        itensEstoque.Property(ie => ie.Codigo).IsRequired();
        itensEstoque.Property(ie => ie.Tipo).HasConversion<string>().IsRequired();
        itensEstoque.Property(ie => ie.Nome).IsRequired();
        itensEstoque.Property(ie => ie.UnidadeMedida).HasConversion<string>().IsRequired();
        itensEstoque.Property(ie => ie.PrecoVenda).HasPrecision(10, 2).IsRequired();
        itensEstoque.Property(ie => ie.Saldo).HasPrecision(10, 3).IsRequired();
        itensEstoque.Property(ie => ie.SaldoReservado).HasPrecision(10, 3).IsRequired();
        itensEstoque.Ignore(ie => ie.SaldoDisponivel);
        SeedItensEstoque(itensEstoque);

        var ordensServico = modelBuilder.Entity<OrdemServicoAggregateRoot>();
        ordensServico.HasKey(os => os.Id);
        ordensServico.HasIndex(os => os.Status);
        ordensServico.Property(os => os.Status).HasConversion<string>().IsRequired();
        ordensServico.Property(os => os.RecebidaEm).IsRequired();
        ordensServico.Property(os => os.EntregueEm);
        ordensServico.Property(os => os.DescartadaEm);
        ordensServico.HasIndex(os => os.TokenAprovacao).IsUnique();
        ordensServico.Property(os => os.TokenAprovacao).IsRequired();
        ordensServico.Ignore(os => os.ValorTotal);
        ordensServico.Ignore(os => os.ItensNecessariosParaExecucao);
        ordensServico.ComplexProperty(os => os.Cliente);
        ordensServico.ComplexProperty(os => os.Veiculo);
        ordensServico
            .HasMany(os => os.Servicos)
            .WithOne()
            .HasForeignKey(ios => ios.IdOrdemServico)
            .OnDelete(DeleteBehavior.Cascade);

        var servicosOrdemServico = modelBuilder.Entity<Servico>();
        servicosOrdemServico.HasKey(ios => ios.Id);
        servicosOrdemServico.HasIndex(ios => ios.Status);
        servicosOrdemServico.Property(ios => ios.Status).HasConversion<string>().IsRequired();
        servicosOrdemServico.Property(ios => ios.Nome).IsRequired();
        servicosOrdemServico.Property(ios => ios.ValorCobrado).HasPrecision(10, 2).IsRequired();
        servicosOrdemServico.ComplexProperty(ios => ios.ServicoCatalogo);
        servicosOrdemServico
            .HasMany(ios => ios.ItensNecessarios)
            .WithOne()
            .HasForeignKey(ieos => ieos.IdItemOrdemServico)
            .OnDelete(DeleteBehavior.Cascade);

        var itensNecessariosOrdemServico = modelBuilder.Entity<ItemNecessario>();
        itensNecessariosOrdemServico.HasKey(inos => inos.Id);
        itensNecessariosOrdemServico.HasIndex(inos => inos.Status);
        itensNecessariosOrdemServico.Property(inos => inos.Status).HasConversion<string>().IsRequired();
        itensNecessariosOrdemServico.Property(inos => inos.Quantidade).HasPrecision(10, 3).IsRequired();
        itensNecessariosOrdemServico.ComplexProperty(inos => inos.ItemEstoque);
        itensNecessariosOrdemServico
            .HasOne<OrdemServicoAggregateRoot>()
            .WithMany()
            .HasForeignKey(ie => ie.IdOrdemServico)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void SeedItensEstoque(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ItemEstoqueAggregateRoot> itensEstoque)
    {
        itensEstoque.HasData(
            new ItemEstoqueAggregateRoot { Id = 1, Codigo = "INS-001", Tipo = ItemTipo.Insumo, Nome = "Óleo Motor Sintético 5W30", UnidadeMedida = UnidadeMedida.Litro, PrecoVenda = 45.90m, Saldo = 50.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 2, Codigo = "INS-002", Tipo = ItemTipo.Insumo, Nome = "Óleo Motor Semissintético 15W40", UnidadeMedida = UnidadeMedida.Litro, PrecoVenda = 38.50m, Saldo = 0.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 3, Codigo = "PEC-001", Tipo = ItemTipo.Peca, Nome = "Filtro de Óleo", UnidadeMedida = UnidadeMedida.Unidade, PrecoVenda = 35.00m, Saldo = 15.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 4, Codigo = "PEC-002", Tipo = ItemTipo.Peca, Nome = "Filtro de Ar do Motor", UnidadeMedida = UnidadeMedida.Unidade, PrecoVenda = 42.00m, Saldo = 20.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 5, Codigo = "PEC-003", Tipo = ItemTipo.Peca, Nome = "Filtro de Cabine (Ar Condicionado)", UnidadeMedida = UnidadeMedida.Unidade, PrecoVenda = 30.00m, Saldo = 0.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 6, Codigo = "PEC-004", Tipo = ItemTipo.Peca, Nome = "Pastilha de Freio Dianteira", UnidadeMedida = UnidadeMedida.Jogo, PrecoVenda = 120.00m, Saldo = 8.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 7, Codigo = "PEC-005", Tipo = ItemTipo.Peca, Nome = "Pastilha de Freio Traseira", UnidadeMedida = UnidadeMedida.Jogo, PrecoVenda = 95.00m, Saldo = 3.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 8, Codigo = "INS-003", Tipo = ItemTipo.Insumo, Nome = "Fluido de Freio DOT 4", UnidadeMedida = UnidadeMedida.Frasco, PrecoVenda = 25.00m, Saldo = 30.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 9, Codigo = "INS-004", Tipo = ItemTipo.Insumo, Nome = "Chumbo para Balanceamento", UnidadeMedida = UnidadeMedida.Kg, PrecoVenda = 18.00m, Saldo = 5.500m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 10, Codigo = "INS-005", Tipo = ItemTipo.Insumo, Nome = "Descarbonizante Spray", UnidadeMedida = UnidadeMedida.Frasco, PrecoVenda = 22.00m, Saldo = 10.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 11, Codigo = "PEC-006", Tipo = ItemTipo.Peca, Nome = "Kit O-ring Bico Injetor", UnidadeMedida = UnidadeMedida.Jogo, PrecoVenda = 15.00m, Saldo = 25.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 12, Codigo = "PEC-016", Tipo = ItemTipo.Peca, Nome = "Correia Dentada", UnidadeMedida = UnidadeMedida.Unidade, PrecoVenda = 85.00m, Saldo = 6.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 13, Codigo = "PEC-017", Tipo = ItemTipo.Peca, Nome = "Tensor da Correia Dentada", UnidadeMedida = UnidadeMedida.Unidade, PrecoVenda = 110.00m, Saldo = 4.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 14, Codigo = "PEC-018", Tipo = ItemTipo.Peca, Nome = "Vela de Ignição", UnidadeMedida = UnidadeMedida.Jogo, PrecoVenda = 140.00m, Saldo = 0.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 15, Codigo = "PEC-007", Tipo = ItemTipo.Peca, Nome = "Bateria 60Ah", UnidadeMedida = UnidadeMedida.Unidade, PrecoVenda = 350.00m, Saldo = 4.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 16, Codigo = "PEC-008", Tipo = ItemTipo.Peca, Nome = "Bateria 45Ah", UnidadeMedida = UnidadeMedida.Unidade, PrecoVenda = 280.00m, Saldo = 0.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 17, Codigo = "PEC-009", Tipo = ItemTipo.Peca, Nome = "Terminal de Bateria", UnidadeMedida = UnidadeMedida.Par, PrecoVenda = 12.00m, Saldo = 40.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 18, Codigo = "PEC-014", Tipo = ItemTipo.Peca, Nome = "Lâmpada H4", UnidadeMedida = UnidadeMedida.Unidade, PrecoVenda = 25.00m, Saldo = 0.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 19, Codigo = "PEC-015", Tipo = ItemTipo.Peca, Nome = "Lâmpada Pingo T10", UnidadeMedida = UnidadeMedida.Par, PrecoVenda = 10.00m, Saldo = 50.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 20, Codigo = "INS-006", Tipo = ItemTipo.Insumo, Nome = "Higienizador Granada", UnidadeMedida = UnidadeMedida.Frasco, PrecoVenda = 28.00m, Saldo = 18.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 21, Codigo = "INS-007", Tipo = ItemTipo.Insumo, Nome = "Gás Refrigerante R134a", UnidadeMedida = UnidadeMedida.Kg, PrecoVenda = 85.00m, Saldo = 3.200m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 22, Codigo = "PEC-010", Tipo = ItemTipo.Peca, Nome = "Amortecedor Dianteiro", UnidadeMedida = UnidadeMedida.Par, PrecoVenda = 450.00m, Saldo = 2.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 23, Codigo = "PEC-011", Tipo = ItemTipo.Peca, Nome = "Kit Batente Amortecedor Dianteiro", UnidadeMedida = UnidadeMedida.Jogo, PrecoVenda = 80.00m, Saldo = 5.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 24, Codigo = "PEC-012", Tipo = ItemTipo.Peca, Nome = "Kit Embreagem Completo", UnidadeMedida = UnidadeMedida.Jogo, PrecoVenda = 650.00m, Saldo = 0.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 25, Codigo = "INS-008", Tipo = ItemTipo.Insumo, Nome = "Fluido de Transmissão Manual", UnidadeMedida = UnidadeMedida.Litro, PrecoVenda = 55.00m, Saldo = 12.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 26, Codigo = "PEC-013", Tipo = ItemTipo.Peca, Nome = "Palheta Limpador Parabrisa", UnidadeMedida = UnidadeMedida.Par, PrecoVenda = 45.00m, Saldo = 10.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 27, Codigo = "INS-009", Tipo = ItemTipo.Insumo, Nome = "Aditivo Radiador Concentrado", UnidadeMedida = UnidadeMedida.Litro, PrecoVenda = 32.00m, Saldo = 24.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 28, Codigo = "INS-010", Tipo = ItemTipo.Insumo, Nome = "Água Desmineralizada", UnidadeMedida = UnidadeMedida.Litro, PrecoVenda = 8.00m, Saldo = 60.000m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 29, Codigo = "INS-011", Tipo = ItemTipo.Insumo, Nome = "Estopa Polimento", UnidadeMedida = UnidadeMedida.Kg, PrecoVenda = 15.00m, Saldo = 2.500m, SaldoReservado = 0m },
            new ItemEstoqueAggregateRoot { Id = 30, Codigo = "INS-012", Tipo = ItemTipo.Insumo, Nome = "Desengraxante Concentrado", UnidadeMedida = UnidadeMedida.Litro, PrecoVenda = 20.00m, Saldo = 15.000m, SaldoReservado = 0m }
        );
    }
}
