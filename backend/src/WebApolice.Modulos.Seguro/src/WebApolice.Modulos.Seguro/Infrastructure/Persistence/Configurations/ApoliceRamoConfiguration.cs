using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence.Configurations;

public class ApoliceRamoConfiguration : IEntityTypeConfiguration<ApoliceRamoModel>
{
    public void Configure(EntityTypeBuilder<ApoliceRamoModel> builder)
    {
        builder.ToTable("apolice_ramo", "seguro");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ApoliceId).HasColumnName("apolice_id").IsRequired();
        
        builder.Property(x => x.TipoRamo).HasColumnName("tipo_ramo").HasMaxLength(40).IsRequired();
        builder.Property(x => x.NumeroApolice).HasColumnName("numero_apolice").HasMaxLength(80);
        builder.Property(x => x.IofPercentual).HasColumnName("iof_percentual").HasPrecision(18, 4);
        
        builder.Property(x => x.Ativo).HasColumnName("ativo").HasDefaultValue(true);
        builder.Property(x => x.LegadoId).HasColumnName("legado_id");
        
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

        // Relacionamentos e índices
        builder.HasIndex(x => x.ApoliceId).HasDatabaseName("ix_apolice_ramo_apolice");
        builder.HasIndex(x => x.TipoRamo).HasDatabaseName("ix_apolice_ramo_tipo_ramo");
        
        builder.HasIndex(x => new { x.ApoliceId, x.TipoRamo })
            .HasDatabaseName("ux_apolice_ramo_ativo")
            .IsUnique()
            .HasFilter("ativo = true");

        builder.HasOne(x => x.Apolice)
            .WithMany(x => x.Ramos)
            .HasForeignKey(x => x.ApoliceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
