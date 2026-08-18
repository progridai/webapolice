using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Configurations;

public class ApoliceHistoricoConfiguration : IEntityTypeConfiguration<ApoliceHistoricoModel>
{
    public void Configure(EntityTypeBuilder<ApoliceHistoricoModel> builder)
    {
        builder.ToTable("apolice_historico", "seguro");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        
        builder.Property(x => x.ApoliceId).HasColumnName("apolice_id").IsRequired();

        builder.Property(x => x.Acao).HasColumnName("acao").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Descricao).HasColumnName("descricao").HasMaxLength(1000);
        
        builder.Property(x => x.UsuarioPublicId).HasColumnName("usuario_public_id");

        builder.Property(x => x.DataAcao).HasColumnName("data_acao").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();

        builder.HasOne(x => x.Apolice)
               .WithMany(x => x.Historicos)
               .HasForeignKey(x => x.ApoliceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
