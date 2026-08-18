using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Configurations;

public class ModuloConfiguration : IEntityTypeConfiguration<ModuloModel>
{
    public void Configure(EntityTypeBuilder<ModuloModel> builder)
    {
        builder.ToTable("modulo", "cadastro");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(x => x.PublicId).HasColumnName("public_id").HasDefaultValueSql("gen_random_uuid()").IsRequired();

        builder.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(150).IsRequired();
        builder.Property(x => x.Descricao).HasColumnName("descricao").HasMaxLength(500);
        
        builder.Property(x => x.Ativo).HasColumnName("ativo").HasDefaultValue(true).IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => x.Nome);
        builder.HasIndex(x => x.Ativo).HasFilter("deleted_at IS NULL");
    }
}
