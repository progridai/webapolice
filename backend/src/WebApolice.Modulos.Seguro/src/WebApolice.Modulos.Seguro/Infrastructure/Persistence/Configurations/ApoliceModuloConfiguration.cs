using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Configurations;

public class ApoliceModuloConfiguration : IEntityTypeConfiguration<ApoliceModuloModel>
{
    public void Configure(EntityTypeBuilder<ApoliceModuloModel> builder)
    {
        builder.ToTable("apolice_modulo", "seguro");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        
        builder.Property(x => x.PublicId)
            .HasColumnName("public_id")
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();

        builder.Property(x => x.ApoliceId).HasColumnName("apolice_id").IsRequired();
        builder.Property(x => x.ModuloId).HasColumnName("modulo_id").IsRequired();

        builder.Property(x => x.DataInicio).HasColumnName("data_inicio");
        builder.Property(x => x.DataFim).HasColumnName("data_fim");
        
        builder.Property(x => x.Ativo).HasColumnName("ativo").HasDefaultValue(true).IsRequired();
        builder.Property(x => x.Observacao).HasColumnName("observacao");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        // Indexes
        builder.HasIndex(x => x.PublicId)
            .HasDatabaseName("ux_apolice_modulo_public_id")
            .IsUnique();

        builder.HasIndex(x => x.ApoliceId)
            .HasDatabaseName("ix_apolice_modulo_apolice_id");
            
        builder.HasIndex(x => x.ModuloId)
            .HasDatabaseName("ix_apolice_modulo_modulo_id");

        // Regra de Duplicidade: Módulo não pode se repetir na apólice (inclui inativos, desde que não deletados)
        builder.HasIndex(x => new { x.ApoliceId, x.ModuloId })
            .HasDatabaseName("ux_apolice_modulo_unico")
            .IsUnique()
            .HasFilter("deleted_at IS NULL");

        // Relacionamento com Apólice
        builder.HasOne(x => x.Apolice)
            .WithMany(x => x.Modulos)
            .HasForeignKey(x => x.ApoliceId)
            .HasConstraintName("fk_apolice_modulo_apolice_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
