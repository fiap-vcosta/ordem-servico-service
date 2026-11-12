using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<OrdemServicoAggregateRoot> OrdensServico { get; set; }
    public DbSet<Servico> ItensServicos { get; set; }
    public DbSet<EventoProcessado> EventosProcessados { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var ordensServico = modelBuilder.Entity<OrdemServicoAggregateRoot>();
        ordensServico.HasKey(os => os.Id);
        ordensServico.Property(os => os.Id).ValueGeneratedNever();
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

        var eventos = modelBuilder.Entity<EventoProcessado>();
        eventos.HasKey(e => e.EventId);
        eventos.Property(e => e.Tipo).IsRequired();
        eventos.Property(e => e.ChaveNegocio).IsRequired();
        eventos.Property(e => e.ProcessadoEm).IsRequired();
        eventos.HasIndex(e => new { e.Tipo, e.ChaveNegocio }).IsUnique();
    }
}
