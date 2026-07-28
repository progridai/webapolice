using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Seguranca.Domain;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Persistence.Configurations;

internal sealed class ModuloConfiguration : IEntityTypeConfiguration<Modulo>
{
    public void Configure(EntityTypeBuilder<Modulo> builder)
    {
        builder.ToTable("modulo", "seguranca");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.PublicId)
               .HasDefaultValueSql("gen_random_uuid()")
               .IsRequired();

        builder.Property(m => m.Codigo).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Nome).IsRequired().HasMaxLength(150);
        builder.Property(m => m.Descricao).HasMaxLength(500);
        builder.Property(m => m.Icone).HasMaxLength(100);
        builder.Property(m => m.Ordem).HasDefaultValue(0);
        builder.Property(m => m.Ativo).HasDefaultValue(true);
        builder.Property(m => m.Habilitado).HasDefaultValue(true).IsRequired();
        builder.Property(m => m.CreatedAt).HasDefaultValueSql("now()").IsRequired();
        builder.Property(m => m.UpdatedAt).HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(m => m.Codigo).IsUnique();
        builder.HasIndex(m => m.PublicId).IsUnique();
    }
}
