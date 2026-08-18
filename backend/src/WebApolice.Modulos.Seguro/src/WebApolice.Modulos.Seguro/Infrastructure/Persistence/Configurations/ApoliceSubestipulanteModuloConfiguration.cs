using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Configurations;

public class ApoliceSubestipulanteModuloConfiguration : IEntityTypeConfiguration<ApoliceSubestipulanteModuloModel>
{
    public void Configure(EntityTypeBuilder<ApoliceSubestipulanteModuloModel> builder)
    {
        builder.ToTable("apolice_subestipulante_modulo", "seguro");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        
        builder.Property(x => x.ApoliceSubestipulanteId).HasColumnName("apolice_subestipulante_id").IsRequired();
        builder.Property(x => x.ModuloId).HasColumnName("modulo_id").IsRequired();

        builder.Property(x => x.DataInicio).HasColumnName("data_inicio");
        builder.Property(x => x.DataFim).HasColumnName("data_fim");
        
        builder.Property(x => x.Ativo).HasColumnName("ativo").HasDefaultValue(true).IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        // Regra Inviolável: O mesmo módulo não se repete no mesmo subestipulante da mesma apólice
        builder.HasIndex(x => new { x.ApoliceSubestipulanteId, x.ModuloId }).IsUnique().HasFilter("deleted_at IS NULL");
        
        builder.HasOne(x => x.ApoliceSubestipulante)
               .WithMany(x => x.Modulos)
               .HasForeignKey(x => x.ApoliceSubestipulanteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
