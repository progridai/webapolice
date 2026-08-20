using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Configurations;

public class SubestipulanteConfiguration : IEntityTypeConfiguration<SubestipulanteModel>
{
    public void Configure(EntityTypeBuilder<SubestipulanteModel> builder)
    {
        builder.ToTable("subestipulante", "cadastro");
        builder.HasKey(x => x.Id).HasName("pk_subestipulante");

        builder.Property(x => x.Id).HasColumnName("id").IsRequired();
        builder.Property(x => x.PublicId).HasColumnName("public_id").IsRequired();
        builder.Property(x => x.PessoaId).HasColumnName("pessoa_id").IsRequired();

        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(80);
        builder.Property(x => x.Ativo).HasColumnName("ativo").IsRequired().HasDefaultValue(true);
        builder.Property(x => x.Observacao).HasColumnName("observacao");
        builder.Property(x => x.LegadoId).HasColumnName("legado_id");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired().HasDefaultValueSql("now()");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => x.PublicId)
               .IsUnique()
               .HasDatabaseName("ix_subestipulante_public_id");

        builder.HasIndex(x => x.PessoaId)
               .HasDatabaseName("ix_subestipulante_pessoa_id");

        builder.HasIndex(x => x.Ativo)
               .HasDatabaseName("ix_subestipulante_ativo")
               .HasFilter("deleted_at IS NULL");

        builder.HasOne(x => x.Pessoa)
               .WithMany()
               .HasForeignKey(x => x.PessoaId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("fk_subestipulante_pessoa_pessoa_id");
    }
}
