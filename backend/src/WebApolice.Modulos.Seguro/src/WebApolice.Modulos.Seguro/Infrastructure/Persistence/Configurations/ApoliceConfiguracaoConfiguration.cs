using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Configurations;

public class ApoliceConfiguracaoConfiguration : IEntityTypeConfiguration<ApoliceConfiguracaoModel>
{
    public void Configure(EntityTypeBuilder<ApoliceConfiguracaoModel> builder)
    {
        builder.ToTable("apolice_configuracao", "seguro");

        // Relação 1:1 onde a PK é a própria FK da Apólice
        builder.HasKey(x => x.ApoliceId);
        
        builder.Property(x => x.ApoliceId)
               .HasColumnName("apolice_id")
               .ValueGeneratedNever()
               .IsRequired();

        builder.Property(x => x.TipoAdesao).HasColumnName("tipo_adesao").HasMaxLength(50);
        builder.Property(x => x.Custeio).HasColumnName("custeio").HasMaxLength(50);
        builder.Property(x => x.CarenciaDias).HasColumnName("carencia_dias");

        builder.Property(x => x.MesBaseReajuste).HasColumnName("mes_base_reajuste");
        builder.Property(x => x.IndiceReajuste).HasColumnName("indice_reajuste").HasMaxLength(50);

        builder.Property(x => x.CobreConjuge).HasColumnName("cobre_conjuge").HasDefaultValue(false).IsRequired();
        builder.Property(x => x.ControlaExcedente).HasColumnName("controla_excedente").HasDefaultValue(false).IsRequired();

        builder.Property(x => x.DiaCorteFaturamento).HasColumnName("dia_corte_faturamento");
        builder.Property(x => x.PrazoAvisoSinistroDias).HasColumnName("prazo_aviso_sinistro_dias");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();

        builder.HasOne(x => x.Apolice)
               .WithOne(x => x.Configuracao)
               .HasForeignKey<ApoliceConfiguracaoModel>(x => x.ApoliceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
