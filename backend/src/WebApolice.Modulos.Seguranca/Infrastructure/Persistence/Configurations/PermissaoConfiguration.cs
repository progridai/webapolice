using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Seguranca.Domain;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Persistence.Configurations;

internal sealed class PermissaoConfiguration : IEntityTypeConfiguration<Permissao>
{
    public void Configure(EntityTypeBuilder<Permissao> builder)
    {
        builder.ToTable("permissao", "seguranca");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PublicId)
               .HasDefaultValueSql("gen_random_uuid()")
               .IsRequired();

        builder.Property(p => p.Codigo).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Nome).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Descricao).HasMaxLength(500);
        builder.Property(p => p.Ativo).HasDefaultValue(true);
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("now()").IsRequired();
        builder.Property(p => p.UpdatedAt).HasDefaultValueSql("now()").IsRequired();

        builder.HasOne(p => p.Recurso)
               .WithMany(r => r.Permissoes)
               .HasForeignKey(p => p.RecursoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.Codigo).IsUnique();
        builder.HasIndex(p => p.PublicId).IsUnique();
    }
}
