using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Seguranca.Domain;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Persistence.Configurations;

internal sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuario", "seguranca");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.PublicId)
               .HasDefaultValueSql("gen_random_uuid()")
               .IsRequired();

        builder.Property(u => u.KeycloakSub).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Username).HasMaxLength(100);
        builder.Property(u => u.Nome).HasMaxLength(150);
        builder.Property(u => u.Email).HasMaxLength(150);
        builder.Property(u => u.Ativo).HasDefaultValue(true);
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("now()").IsRequired();
        builder.Property(u => u.UpdatedAt).HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(u => u.KeycloakSub).IsUnique();
        builder.HasIndex(u => u.PublicId).IsUnique();
    }
}
