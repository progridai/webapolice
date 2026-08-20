using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Configurations;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Configurations;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

public partial class SeguroDbContext
{
    public virtual DbSet<ApoliceModel> Apolices { get; set; } = null!;
    public virtual DbSet<ApoliceConfiguracaoModel> ApoliceConfiguracoes { get; set; } = null!;
    public virtual DbSet<ApoliceHistoricoModel> ApoliceHistoricos { get; set; } = null!;
    public virtual DbSet<ApoliceProdutoModel> ApoliceProdutos { get; set; } = null!;
    public virtual DbSet<ApolicePlanoModel> ApolicePlanos { get; set; } = null!;
    public virtual DbSet<ApoliceCoberturaModel> ApoliceCoberturas { get; set; } = null!;
    public virtual DbSet<ApoliceRamoModel> ApoliceRamos { get; set; } = null!;
    public virtual DbSet<ApoliceSubestipulanteModel> ApoliceSubestipulantes { get; set; } = null!;
    public virtual DbSet<ApoliceSubestipulanteModuloModel> ApoliceSubestipulanteModulos { get; set; } = null!;
    public virtual DbSet<ApoliceVidaModel> ApoliceVidas { get; set; } = null!;

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new RamoConfiguration());
        modelBuilder.ApplyConfiguration(new ApoliceConfiguration());
        modelBuilder.ApplyConfiguration(new ApoliceConfiguracaoConfiguration());
        modelBuilder.ApplyConfiguration(new ApoliceHistoricoConfiguration());
        modelBuilder.ApplyConfiguration(new ApoliceProdutoConfiguration());
        modelBuilder.ApplyConfiguration(new ApolicePlanoConfiguration());
        modelBuilder.ApplyConfiguration(new ApoliceCoberturaConfiguration());
        modelBuilder.ApplyConfiguration(new ApoliceRamoConfiguration());
        modelBuilder.ApplyConfiguration(new ApoliceSubestipulanteConfiguration());
        modelBuilder.ApplyConfiguration(new ApoliceSubestipulanteModuloConfiguration());
        modelBuilder.ApplyConfiguration(new ApoliceVidaConfiguration());
        
        // Atualiza a configuração do Propostum
        modelBuilder.Entity<Propostum>(entity => 
        {
            entity.Property(e => e.ApoliceId).HasColumnName("apolice_id");
            entity.Property(e => e.ApoliceVidaId).HasColumnName("apolice_vida_id");
            
            entity.HasIndex(e => e.ApoliceId, "ix_proposta_apolice")
                  .HasFilter("apolice_id IS NOT NULL");
                  
            entity.HasIndex(e => e.ApoliceVidaId, "ix_proposta_apolice_vida")
                  .HasFilter("apolice_vida_id IS NOT NULL");
            
            entity.HasOne(d => d.Apolice)
                .WithMany(p => p.Propostas)
                .HasForeignKey(d => d.ApoliceId)
                .HasConstraintName("proposta_apolice_id_fkey")
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(d => d.ApoliceVida)
                .WithMany(p => p.Propostas)
                .HasForeignKey(d => d.ApoliceVidaId)
                .HasConstraintName("proposta_apolice_vida_id_fkey")
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
