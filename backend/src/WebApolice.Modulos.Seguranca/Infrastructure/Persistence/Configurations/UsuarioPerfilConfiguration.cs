using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Seguranca.Domain.Relacionamentos;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Persistence.Configurations;

internal sealed class UsuarioPerfilConfiguration : IEntityTypeConfiguration<UsuarioPerfil>
{
    public void Configure(EntityTypeBuilder<UsuarioPerfil> builder)
    {
        builder.ToTable("usuario_perfil", "seguranca");
        builder.HasKey(up => new { up.UsuarioId, up.PerfilId });

        builder.Property(up => up.CreatedAt).HasDefaultValueSql("now()").IsRequired();

        builder.HasOne(up => up.Usuario)
               .WithMany(u => u.Perfis)
               .HasForeignKey(up => up.UsuarioId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(up => up.Perfil)
               .WithMany(p => p.Usuarios)
               .HasForeignKey(up => up.PerfilId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(up => up.AtribuidoPorUsuario)
               .WithMany()
               .HasForeignKey(up => up.AtribuidoPorUsuarioId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
