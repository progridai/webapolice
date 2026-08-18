using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence.Configurations;

public class ApoliceConfiguration : IEntityTypeConfiguration<ApoliceModel>
{
    public void Configure(EntityTypeBuilder<ApoliceModel> builder)
    {
        builder.ToTable("apolice", "seguro");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PublicId)
            .HasColumnName("public_id")
            .HasDefaultValueSql("gen_random_uuid()");
            
        builder.Property(x => x.EstipulanteId).HasColumnName("estipulante_id").IsRequired();
        builder.Property(x => x.SeguradoraId).HasColumnName("seguradora_id").IsRequired();
        builder.Property(x => x.CorretoraId).HasColumnName("corretora_id");
        
        builder.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(200);
        builder.Property(x => x.DataInicioVigencia).HasColumnName("data_inicio_vigencia").IsRequired();
        builder.Property(x => x.DataFimVigencia).HasColumnName("data_fim_vigencia");
        builder.Property(x => x.DataAniversario).HasColumnName("data_aniversario");
        
        builder.Property(x => x.ApoliceOrigemId).HasColumnName("apolice_origem_id");
        builder.Property(x => x.Versao).HasColumnName("versao").HasDefaultValue(1);
        
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(40).HasDefaultValue("ativa");
        builder.Property(x => x.Ativo).HasColumnName("ativo").HasDefaultValue(true);
        
        builder.Property(x => x.LegadoId).HasColumnName("legado_id");
        builder.Property(x => x.Observacao).HasColumnName("observacao");
        
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        // Relacionamentos e índices
        builder.HasIndex(x => x.EstipulanteId).HasDatabaseName("ix_apolice_estipulante");
        builder.HasIndex(x => x.SeguradoraId).HasDatabaseName("ix_apolice_seguradora");
        
        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_apolice_status")
            .HasFilter("deleted_at IS NULL");
            
        builder.HasIndex(x => new { x.DataInicioVigencia, x.DataFimVigencia })
            .HasDatabaseName("ix_apolice_vigencia");
            
        builder.HasIndex(x => x.LegadoId)
            .HasDatabaseName("ux_apolice_legado")
            .IsUnique()
            .HasFilter("legado_id IS NOT NULL");

        builder.HasOne(x => x.ApoliceOrigem)
            .WithMany(x => x.Renovacoes)
            .HasForeignKey(x => x.ApoliceOrigemId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
