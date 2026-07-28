using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Seguranca.Domain;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Persistence.Configurations;

internal sealed class PerfilConfiguration : IEntityTypeConfiguration<Perfil>
{
    public void Configure(EntityTypeBuilder<Perfil> builder)
    {
        builder.ToTable("perfil", "seguranca");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PublicId)
               .HasDefaultValueSql("gen_random_uuid()")
               .IsRequired();

        builder.Property(p => p.Codigo).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Nome).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Descricao).HasMaxLength(500);
        builder.Property(p => p.PerfilSistema).HasDefaultValue(false);
        builder.Property(p => p.AcessoTotal).HasDefaultValue(false);
        builder.Property(p => p.Ativo).HasDefaultValue(true);
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("now()").IsRequired();
        builder.Property(p => p.UpdatedAt).HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(p => p.Codigo).IsUnique();
        builder.HasIndex(p => p.PublicId).IsUnique();
    }
}
