using Microsoft.EntityFrameworkCore;
using API.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Produto> Produtos { get; set; }

    public DbSet<Solicitacao> Solicitacoes { get; set; }

    public DbSet<SolicitacaoItem> SolicitacaoItens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Solicitacao>()
            .HasMany(x => x.Itens)
            .WithOne(x => x.Solicitacao)
            .HasForeignKey(x => x.SolicitacaoId);

        modelBuilder.Entity<SolicitacaoItem>()
            .HasOne(x => x.Produto)
            .WithMany()
            .HasForeignKey(x => x.ProdutoId);
    }
}