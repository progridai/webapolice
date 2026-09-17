using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Configurations;

public class ApoliceSubgrupoConfiguration : IEntityTypeConfiguration<ApoliceSubgrupoModel>
{
    public void Configure(EntityTypeBuilder<ApoliceSubgrupoModel> builder)
    {
        builder.ToTable("apolice_subgrupo", "seguro");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(x => x.PublicId)
            .HasColumnName("public_id")
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();

        builder.Property(x => x.ApoliceId).HasColumnName("apolice_id").IsRequired();

        builder.Property(x => x.Nome)
            .HasColumnName("nome")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Observacao).HasColumnName("observacao");

        builder.Property(x => x.Ativo).HasColumnName("ativo").HasDefaultValue(true).IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        // Índices
        builder.HasIndex(x => x.PublicId)
            .HasDatabaseName("ux_apolice_subgrupo_public_id")
            .IsUnique();

        builder.HasIndex(x => x.ApoliceId)
            .HasDatabaseName("ix_apolice_subgrupo_apolice_id");

        // FK para Apólice — Subgrupo pertence a uma única Apólice
        builder.HasOne(x => x.Apolice)
            .WithMany()
            .HasForeignKey(x => x.ApoliceId)
            .HasConstraintName("fk_apolice_subgrupo_apolice_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
