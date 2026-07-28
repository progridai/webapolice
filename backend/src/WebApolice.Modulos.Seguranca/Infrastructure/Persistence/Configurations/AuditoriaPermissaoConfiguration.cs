using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Seguranca.Domain.Auditoria;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Persistence.Configurations;

internal sealed class AuditoriaPermissaoConfiguration : IEntityTypeConfiguration<AuditoriaPermissao>
{
    public void Configure(EntityTypeBuilder<AuditoriaPermissao> builder)
    {
        builder.ToTable("auditoria_permissao", "seguranca");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.PublicId)
               .HasDefaultValueSql("gen_random_uuid()")
               .IsRequired();

        builder.Property(a => a.Acao).IsRequired().HasMaxLength(50);
        builder.Property(a => a.EntidadeTipo).IsRequired().HasMaxLength(50);
        builder.Property(a => a.DadosAnteriores).HasColumnType("jsonb");
        builder.Property(a => a.DadosNovos).HasColumnType("jsonb");
        builder.Property(a => a.Motivo).HasMaxLength(500);
        builder.Property(a => a.IpOrigem).HasMaxLength(45);
        builder.Property(a => a.UserAgent).HasMaxLength(255);
        builder.Property(a => a.CorrelationId).HasMaxLength(100);
        builder.Property(a => a.CreatedAt).HasDefaultValueSql("now()").IsRequired();

        builder.HasOne(a => a.UsuarioExecutor)
               .WithMany()
               .HasForeignKey(a => a.UsuarioExecutorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.UsuarioAfetado)
               .WithMany()
               .HasForeignKey(a => a.UsuarioAfetadoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Perfil)
               .WithMany()
               .HasForeignKey(a => a.PerfilId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Permissao)
               .WithMany()
               .HasForeignKey(a => a.PermissaoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.PublicId).IsUnique();
        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => a.Acao);
        builder.HasIndex(a => a.UsuarioExecutorId);
        builder.HasIndex(a => a.UsuarioAfetadoId);
        builder.HasIndex(a => a.PerfilId);
        builder.HasIndex(a => a.PermissaoId);
        builder.HasIndex(a => a.CorrelationId);
    }
}
