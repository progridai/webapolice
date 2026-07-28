using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Seguranca.Domain;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Persistence.Configurations;

internal sealed class RecursoConfiguration : IEntityTypeConfiguration<Recurso>
{
    public void Configure(EntityTypeBuilder<Recurso> builder)
    {
        builder.ToTable("recurso", "seguranca");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.PublicId)
               .HasDefaultValueSql("gen_random_uuid()")
               .IsRequired();

        builder.Property(r => r.Codigo).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Nome).IsRequired().HasMaxLength(150);
        builder.Property(r => r.Descricao).HasMaxLength(500);
        builder.Property(r => r.RotaFrontend).HasMaxLength(255);
        builder.Property(r => r.Ordem).HasDefaultValue(0);
        builder.Property(r => r.Ativo).HasDefaultValue(true);
        builder.Property(r => r.CreatedAt).HasDefaultValueSql("now()").IsRequired();
        builder.Property(r => r.UpdatedAt).HasDefaultValueSql("now()").IsRequired();

        builder.HasOne(r => r.Modulo)
               .WithMany(m => m.Recursos)
               .HasForeignKey(r => r.ModuloId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.ModuloId, r.Codigo }).IsUnique();
        builder.HasIndex(r => r.PublicId).IsUnique();
    }
}
