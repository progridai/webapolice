using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Seguranca.Domain.Relacionamentos;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Persistence.Configurations;

internal sealed class PerfilPermissaoConfiguration : IEntityTypeConfiguration<PerfilPermissao>
{
    public void Configure(EntityTypeBuilder<PerfilPermissao> builder)
    {
        builder.ToTable("perfil_permissao", "seguranca");
        builder.HasKey(pp => new { pp.PerfilId, pp.PermissaoId });

        builder.Property(pp => pp.CreatedAt).HasDefaultValueSql("now()").IsRequired();

        builder.HasOne(pp => pp.Perfil)
               .WithMany(p => p.Permissoes)
               .HasForeignKey(pp => pp.PerfilId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pp => pp.Permissao)
               .WithMany()
               .HasForeignKey(pp => pp.PermissaoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pp => pp.AtribuidoPorUsuario)
               .WithMany()
               .HasForeignKey(pp => pp.AtribuidoPorUsuarioId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
