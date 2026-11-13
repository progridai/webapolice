using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Seguro.Domain.Modelos;

namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence.Configurations;

public sealed class PropostaOperacionalConfiguration : IEntityTypeConfiguration<Propostum>
{
    public void Configure(EntityTypeBuilder<Propostum> entity)
    {
        entity.Property(p => p.DataCancelamentoVigencia).HasColumnName("data_cancelamento_vigencia");
        entity.Property(p => p.CancelamentoSolicitadoEm).HasColumnName("cancelamento_solicitado_em").HasColumnType("timestamp with time zone");
        entity.Property(p => p.DescartadaEm).HasColumnName("descartada_em").HasColumnType("timestamp with time zone");
        entity.Property(p => p.CanceladaEm).HasColumnName("cancelada_em").HasColumnType("timestamp with time zone");
        entity.Property(p => p.MotivoPendenciaVigencia).HasColumnName("motivo_pendencia_vigencia").HasMaxLength(1000);
        entity.Property(p => p.PendenciaVigenciaEm).HasColumnName("pendencia_vigencia_em").HasColumnType("timestamp with time zone");
        entity.Property(p => p.ConfirmadaEm).HasColumnName("confirmada_em").HasColumnType("timestamp with time zone");
        entity.Property(p => p.SubstituidaEm).HasColumnName("substituida_em").HasColumnType("timestamp with time zone");
        entity.Property(p => p.DataInicioVigencia).HasColumnName("data_inicio_vigencia");
        entity.Property(p => p.DataFimVigencia).HasColumnName("data_fim_vigencia");
        entity.HasIndex(p => p.ApoliceVidaId, "ux_proposta_nativa_vigente_vida").HasDatabaseName("ux_proposta_nativa_vigente_vida").IsUnique()
            .HasFilter("legado_id IS NULL AND vigente AND deleted_at IS NULL AND apolice_vida_id IS NOT NULL");
        entity.Property(p => p.RevisaoPreenchimento).HasColumnName("revisao_preenchimento").HasDefaultValue(0).IsConcurrencyToken();
        entity.Property(p => p.ApoliceSubgrupoId).HasColumnName("apolice_subgrupo_id");
        entity.Property(p => p.ApoliceModuloId).HasColumnName("apolice_modulo_id");
        entity.Property(p => p.SubgrupoNomeRegistrado).HasColumnName("subgrupo_nome_registrado").HasMaxLength(200);
        entity.Property(p => p.ModuloNomeRegistrado).HasColumnName("modulo_nome_registrado").HasMaxLength(200);
        entity.HasOne(p => p.ApoliceSubgrupo).WithMany().HasForeignKey(p => p.ApoliceSubgrupoId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(p => p.ApoliceModulo).WithMany().HasForeignKey(p => p.ApoliceModuloId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PropostaBeneficiarioOperacionalConfiguration : IEntityTypeConfiguration<PropostaBeneficiario>
{
    public void Configure(EntityTypeBuilder<PropostaBeneficiario> entity)
    {
        entity.Property(b => b.PublicId).HasColumnName("public_id").HasDefaultValueSql("gen_random_uuid()");
        entity.HasIndex(b => b.PublicId).IsUnique();
        entity.Property(b => b.ParentescoRegistrado).HasColumnName("parentesco_registrado").HasMaxLength(100);
    }
}

public sealed class PropostaItemOperacionalConfiguration : IEntityTypeConfiguration<PropostaItem>
{
    public void Configure(EntityTypeBuilder<PropostaItem> entity)
    {
        entity.Property(p => p.ApoliceModuloPlanoId).HasColumnName("apolice_modulo_plano_id");
        entity.Property(p => p.PlanoNomeRegistrado).HasColumnName("plano_nome_registrado").HasMaxLength(200);
        entity.Property(p => p.PlanoPaga).HasColumnName("plano_paga");
        entity.Property(p => p.PlanoReajuste).HasColumnName("plano_reajuste");
        entity.HasOne(p => p.ApoliceModuloPlano).WithMany().HasForeignKey(p => p.ApoliceModuloPlanoId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PropostaCoberturaOperacionalConfiguration : IEntityTypeConfiguration<PropostaCobertura>
{
    public void Configure(EntityTypeBuilder<PropostaCobertura> entity)
    {
        entity.Property(p => p.PremioAjustado).HasColumnName("premio_ajustado").HasDefaultValue(false);
        entity.Property(p => p.PremioTitularInicial).HasColumnName("premio_titular_inicial").HasPrecision(18, 2);
        entity.Property(p => p.PremioConjugeInicial).HasColumnName("premio_conjuge_inicial").HasPrecision(18, 2);
        entity.Property(p => p.JustificativaAjustePremio).HasColumnName("justificativa_ajuste_premio").HasMaxLength(1000);
        entity.Property(p => p.PublicId).HasColumnName("public_id").HasDefaultValueSql("gen_random_uuid()");
        entity.HasIndex(p => p.PublicId).IsUnique();
        entity.Property(p => p.ApoliceModuloCoberturaId).HasColumnName("apolice_modulo_cobertura_id");
        entity.Property(p => p.CoberturaNomeRegistrado).HasColumnName("cobertura_nome_registrado").HasMaxLength(200);
        entity.Property(p => p.CoberturaNomeReduzidoRegistrado).HasColumnName("cobertura_nome_reduzido_registrado").HasMaxLength(200);
        entity.Property(p => p.CoberturaBasicaRegistrada).HasColumnName("cobertura_basica_registrada").HasMaxLength(50);
        entity.Property(p => p.CoberturaReajuste).HasColumnName("cobertura_reajuste");
        entity.HasOne(p => p.ApoliceModuloCobertura).WithMany().HasForeignKey(p => p.ApoliceModuloCoberturaId).OnDelete(DeleteBehavior.Restrict);
    }
}
