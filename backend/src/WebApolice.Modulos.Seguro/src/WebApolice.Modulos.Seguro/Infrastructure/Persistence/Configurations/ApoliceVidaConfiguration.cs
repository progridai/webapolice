using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence.Configurations;

public class ApoliceVidaConfiguration : IEntityTypeConfiguration<ApoliceVidaModel>
{
    public void Configure(EntityTypeBuilder<ApoliceVidaModel> builder)
    {
        builder.ToTable("apolice_vida", "seguro");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PublicId)
            .HasColumnName("public_id")
            .HasDefaultValueSql("gen_random_uuid()");
            
        builder.Property(x => x.ApoliceId).HasColumnName("apolice_id").IsRequired();
        builder.Property(x => x.ApoliceSubestipulanteId).HasColumnName("apolice_subestipulante_id");
        builder.Property(x => x.ApoliceSubestipulanteModuloId).HasColumnName("apolice_subestipulante_modulo_id");
        builder.Property(x => x.ClienteId).HasColumnName("cliente_id").IsRequired();
        builder.Property(x => x.ClienteVinculoId).HasColumnName("cliente_vinculo_id");
        
        builder.Property(x => x.DataInicioVigencia).HasColumnName("data_inicio_vigencia");
        builder.Property(x => x.DataFimVigencia).HasColumnName("data_fim_vigencia");
        
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(40).HasDefaultValue("ativa");
        builder.Property(x => x.Ativo).HasColumnName("ativo").HasDefaultValue(true);
        builder.Property(x => x.Origem).HasColumnName("origem").HasMaxLength(80);
        
        builder.Property(x => x.LegadoId).HasColumnName("legado_id");
        builder.Property(x => x.Observacao).HasColumnName("observacao");
        
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        // Relacionamentos e índices
        builder.HasIndex(x => x.ApoliceId).HasDatabaseName("ix_apolice_vida_apolice");
        builder.HasIndex(x => x.ClienteId).HasDatabaseName("ix_apolice_vida_cliente");
        
        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_apolice_vida_status")
            .HasFilter("deleted_at IS NULL");
            
        builder.HasIndex(x => new { x.DataInicioVigencia, x.DataFimVigencia })
            .HasDatabaseName("ix_apolice_vida_vigencia");
            
        builder.HasIndex(x => x.ApoliceSubestipulanteId)
            .HasDatabaseName("ix_apolice_vida_subestip")
            .HasFilter("apolice_subestipulante_id IS NOT NULL");
            
        builder.HasIndex(x => x.LegadoId)
            .HasDatabaseName("ux_apolice_vida_legado")
            .IsUnique()
            .HasFilter("legado_id IS NOT NULL");

        builder.HasOne(x => x.Apolice)
            .WithMany(x => x.Vidas)
            .HasForeignKey(x => x.ApoliceId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(x => x.ApoliceSubestipulante)
               .WithMany(x => x.Vidas)
               .HasForeignKey(x => x.ApoliceSubestipulanteId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ApoliceSubestipulanteModulo)
               .WithMany(x => x.Vidas)
               .HasForeignKey(x => x.ApoliceSubestipulanteModuloId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
