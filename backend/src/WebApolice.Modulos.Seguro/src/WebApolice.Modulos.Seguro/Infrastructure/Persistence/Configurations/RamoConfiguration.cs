using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Configurations;

public class RamoConfiguration : IEntityTypeConfiguration<RamoModel>
{
    public void Configure(EntityTypeBuilder<RamoModel> builder)
    {
        builder.ToTable("ramo", "seguro");
        
        builder.HasKey(r => r.Id).HasName("pk_ramo");
        builder.Property(r => r.Id).HasColumnName("id").UseIdentityColumn();
        
        builder.Property(r => r.PublicId).HasColumnName("public_id").IsRequired();
        builder.HasIndex(r => r.PublicId).IsUnique().HasDatabaseName("ux_ramo_public_id");
        
        builder.Property(r => r.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(r => r.Nome).HasColumnName("nome").HasMaxLength(150).IsRequired();
        builder.Property(r => r.Descricao).HasColumnName("descricao").HasMaxLength(500);
        
        builder.Property(r => r.Ativo).HasColumnName("ativo").IsRequired();
        
        builder.HasIndex(r => r.Codigo).HasDatabaseName("ux_ramo_codigo").IsUnique();
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at").IsRequired();
    }
}
