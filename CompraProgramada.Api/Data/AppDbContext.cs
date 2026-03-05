using CompraProgramada.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CompraProgramada.Api.Data {
    
    public class AppDbContext : DbContext {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Add Tabelas para o DB
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<HistoricoValorMensal> HistoricoValoresMensais { get; set; }
        public DbSet<ContaGrafica> ContasGraficas { get; set; }
        public DbSet<Custodia> Custodias { get; set; }
        public DbSet<TopFive> TopFive { get; set; }
        public DbSet<ItemCesta> ItensCesta { get; set; }
        public DbSet<OrdemCompra> OrdensCompra { get; set; }
        public DbSet<Distribuicao> Distribuicoes { get; set; }
        public DbSet<HistoricoVenda> HistoricoVendas { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Cliente>()
                .HasOne(cl => cl.ContaGrafica)
                .WithOne(cg => cg.Cliente)
                .HasForeignKey<ContaGrafica>(cg => cg.ClienteId);

            modelBuilder.Entity<ContaGrafica>()
                .HasMany(cg => cg.Custodias)
                .WithOne(ct => ct.ContaGrafica)
                .HasForeignKey(ct => ct.ContaGraficaId);

            // Define os NOT NULL NO DB
            modelBuilder.Entity<Cliente>().HasIndex(cl => cl.Cpf).IsUnique();
            modelBuilder.Entity<ContaGrafica>().HasIndex(cg => cg.NumeroConta).IsUnique();
        }
    }
}
