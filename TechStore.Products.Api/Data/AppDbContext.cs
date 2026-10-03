using Microsoft.EntityFrameworkCore;
using TechStore.Products.Api.Models;

namespace TechStore.Products.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var produto = modelBuilder.Entity<Produto>();

        produto.ToTable("Produtos", tabela =>
        {
            tabela.HasCheckConstraint(
                "CK_Produtos_Preco_Positivo",
                "[Preco] > 0");
        });

        produto.HasKey(p => p.Id);

        produto.Property(p => p.Codigo)
            .HasMaxLength(50)
            .IsRequired();

        produto.HasIndex(p => p.Codigo)
            .IsUnique();

        produto.Property(p => p.Nome)
            .HasMaxLength(150)
            .IsRequired();

        produto.Property(p => p.Descricao)
            .HasMaxLength(1000);

        produto.Property(p => p.Preco)
            .HasPrecision(18, 2);

        produto.Property(p => p.Categoria)
            .HasMaxLength(100)
            .IsRequired();

        produto.Property(p => p.Ativo)
            .IsRequired();

        produto.Property(p => p.CriadoEmUtc)
            .IsRequired();
    }
}