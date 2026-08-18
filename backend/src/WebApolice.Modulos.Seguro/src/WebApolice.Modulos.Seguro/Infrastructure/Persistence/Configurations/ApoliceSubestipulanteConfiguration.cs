using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence.Configurations;

public class ApoliceSubestipulanteConfiguration : IEntityTypeConfiguration<ApoliceSubestipulanteModel>
{
    public void Configure(EntityTypeBuilder<ApoliceSubestipulanteModel> builder)
    {
        builder.ToTable("apolice_subestipulante", "seguro");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ApoliceId).HasColumnName("apolice_id").IsRequired();
        builder.Property(x => x.SubestipulanteId).HasColumnName("subestipulante_id").IsRequired();
        
        builder.Property(x => x.DataInicio).HasColumnName("data_inicio");
        builder.Property(x => x.DataFim).HasColumnName("data_fim");
        
        builder.Property(x => x.Ativo).HasColumnName("ativo").HasDefaultValue(true);
        builder.Property(x => x.LegadoId).HasColumnName("legado_id");
        
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        // Relacionamentos e índices
        builder.HasIndex(x => x.ApoliceId).HasDatabaseName("ix_apolice_sub_apolice");
        builder.HasIndex(x => x.SubestipulanteId).HasDatabaseName("ix_apolice_sub_subestipulante");
        
        builder.HasIndex(x => new { x.ApoliceId, x.SubestipulanteId })
            .HasDatabaseName("ux_apolice_sub_ativo")
            .IsUnique()
            .HasFilter("ativo = true AND deleted_at IS NULL");

        builder.HasOne(x => x.Apolice)
            .WithMany(x => x.Subestipulantes)
            .HasForeignKey(x => x.ApoliceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
