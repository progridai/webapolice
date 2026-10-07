using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence.Configurations;

public sealed class ApoliceModuloPlanoConfiguration : IEntityTypeConfiguration<ApoliceModuloPlanoModel>
{
    public void Configure(EntityTypeBuilder<ApoliceModuloPlanoModel> b)
    {
        b.ToTable("apolice_modulo_plano", "seguro", t => t.HasCheckConstraint("ck_modulo_plano_nome", "length(btrim(nome)) > 0"));
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).UseIdentityAlwaysColumn();
        b.Property(e => e.PublicId).HasDefaultValueSql("gen_random_uuid()");
        b.HasIndex(e => e.PublicId).IsUnique();
        b.Property(e => e.Nome).HasMaxLength(150).IsRequired();
        b.Property(e => e.Ramo).HasMaxLength(80);
        b.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        b.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        // Um único Plano por vínculo, inclusive quando inativo.
        b.HasIndex(e => e.ApoliceModuloId).IsUnique();
        b.HasOne(e => e.ApoliceModulo).WithOne().HasForeignKey<ApoliceModuloPlanoModel>(e => e.ApoliceModuloId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(e => e.Coberturas).WithOne(e => e.Plano).HasForeignKey(e => e.ApoliceModuloPlanoId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ApoliceModuloCoberturaConfiguration : IEntityTypeConfiguration<ApoliceModuloCoberturaModel>
{
    public void Configure(EntityTypeBuilder<ApoliceModuloCoberturaModel> b)
    {
        b.ToTable("apolice_modulo_cobertura", "seguro", t =>
        {
            t.HasCheckConstraint("ck_modulo_cobertura_premios", "premio_titular >= 0 AND premio_conjuge >= 0");
        });
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).UseIdentityAlwaysColumn();
        b.Property(e => e.PublicId).HasDefaultValueSql("gen_random_uuid()");
        b.HasIndex(e => e.PublicId).IsUnique();
        b.HasOne(e => e.Cobertura).WithMany().HasForeignKey(e => e.CoberturaId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(e => new { e.ApoliceModuloPlanoId, e.CoberturaId }).IsUnique();
        b.Property(e => e.PremioTitular).HasPrecision(18, 2);
        b.Property(e => e.PremioConjuge).HasPrecision(18, 2);
        b.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        b.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
    }
}
