using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Configurations;

public class EstipulanteConfiguracaoConfiguration : IEntityTypeConfiguration<EstipulanteConfiguracaoModel>
{
    public void Configure(EntityTypeBuilder<EstipulanteConfiguracaoModel> builder)
    {
        builder.ToTable("estipulante_configuracao", "cadastro", t => t.ExcludeFromMigrations());
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.EstipulanteId).HasColumnName("estipulante_id");
        builder.Property(x => x.PermitePropostas).HasColumnName("permite_propostas");
        builder.Property(x => x.ControlaComissao).HasColumnName("controla_comissao");
        builder.Property(x => x.DataInicioVigencia).HasColumnName("data_inicio_vigencia");
        builder.Property(x => x.DataFimVigencia).HasColumnName("data_fim_vigencia");
        builder.Property(x => x.DataAniversario).HasColumnName("data_aniversario");
        builder.Property(x => x.DataUltimoReajuste).HasColumnName("data_ultimo_reajuste");
        builder.Property(x => x.DataBaseReajuste).HasColumnName("data_base_reajuste");
        builder.Property(x => x.DataLimiteReajuste).HasColumnName("data_limite_reajuste");
        builder.Property(x => x.DiasAvisoReajuste).HasColumnName("dias_aviso_reajuste");
        builder.Property(x => x.Carencia).HasColumnName("carencia");
        builder.Property(x => x.AdesaoPor).HasColumnName("adesao_por");
        builder.Property(x => x.Custeio).HasColumnName("custeio");
        builder.Property(x => x.Adesao).HasColumnName("adesao");
        builder.Property(x => x.FaixaEtariaInicio).HasColumnName("faixa_etaria_inicio");
        builder.Property(x => x.FaixaEtariaFim).HasColumnName("faixa_etaria_fim");
        builder.Property(x => x.CancelaEstipulanteId).HasColumnName("cancela_estipulante_id");
        
        // Campos com DEFAULT no banco
        builder.Property(x => x.DesconsiderarPropostaAtiva)
            .HasColumnName("desconsiderar_proposta_ativa")
            .HasDefaultValue(false);
        builder.Property(x => x.PermitirProtocoloDuplicado)
            .HasColumnName("permitir_protocolo_duplicado")
            .HasDefaultValue(false);
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");
    }
}
