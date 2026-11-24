using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.Domain.Modelos;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

public partial class SeguroDbContext
{
    public DbSet<PropostaFinanceiro> PropostasFinanceiro { get; set; } = null!;
    public DbSet<ParcelaCompetencia> ParcelasCompetencias { get; set; } = null!;

    private static void ConfigurarFinanceiro(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PropostaFinanceiro>(b =>
        {
            b.ToTable("proposta_financeiro", "seguro", t => t.HasCheckConstraint("ck_proposta_financeiro_valores",
                "premio_liquido >= 0 AND iof_percentual BETWEEN 0 AND 100 AND iof_valor >= 0 AND total_mensal = premio_liquido + iof_valor AND competencia_inicial % 100 BETWEEN 1 AND 12 AND competencia_inicial BETWEEN 101 AND 999912 AND competencia_referencia % 100 BETWEEN 1 AND 12 AND competencia_referencia BETWEEN 101 AND 999912"));
            b.HasKey(x => x.Id); b.HasIndex(x => x.PublicId).IsUnique(); b.HasIndex(x => x.PropostaId).IsUnique();
            b.Property(x => x.CondicoesHash).HasMaxLength(64).IsRequired();
            b.Property(x => x.PremioLiquido).HasPrecision(18, 2); b.Property(x => x.IofValor).HasPrecision(18, 2);
            b.Property(x => x.TotalMensal).HasPrecision(18, 2); b.Property(x => x.IofPercentual).HasPrecision(18, 4);
            b.HasOne(x => x.Proposta).WithOne().HasForeignKey<PropostaFinanceiro>(x => x.PropostaId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.ApoliceRamo).WithMany().HasForeignKey(x => x.ApoliceRamoId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ParcelaCompetencia>(b =>
        {
            b.ToTable("parcela_competencia", "seguro", t => t.HasCheckConstraint("ck_parcela_competencia_mes",
                "competencia % 100 BETWEEN 1 AND 12 AND competencia BETWEEN 101 AND 999912 AND revisao >= 0 AND (fechada_em IS NULL OR gerada_em IS NOT NULL)"));
            b.HasKey(x => x.Id); b.HasIndex(x => x.PublicId).IsUnique(); b.HasIndex(x => new { x.ApoliceId, x.Competencia }).IsUnique();
            b.HasOne(x => x.Apolice).WithMany().HasForeignKey(x => x.ApoliceId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<PropostaMovimento>(b =>
        {
            const string filtro = "legado_id IS NULL AND classificacao = 'PARCELA_MENSAL'";
            b.HasIndex(x => new { x.ApoliceVidaId, x.CompetenciaInt }).IsUnique().HasDatabaseName("ux_parcela_vida_competencia").HasFilter(filtro);
            b.HasIndex(x => new { x.ApoliceVidaId, x.Parcela }).IsUnique().HasDatabaseName("ux_parcela_vida_numero").HasFilter(filtro);
            b.HasOne(x => x.ApoliceVida).WithMany().HasForeignKey(x => x.ApoliceVidaId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.PropostaFinanceiro).WithMany().HasForeignKey(x => x.PropostaFinanceiroId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.ParcelaCompetencia).WithMany().HasForeignKey(x => x.ParcelaCompetenciaId).OnDelete(DeleteBehavior.Restrict);
            b.ToTable("proposta_movimento", "seguro", t => t.HasCheckConstraint("ck_parcela_mensal_nativa",
                "legado_id IS NOT NULL OR classificacao <> 'PARCELA_MENSAL' OR (public_id IS NOT NULL AND proposta_id IS NOT NULL AND apolice_vida_id IS NOT NULL AND proposta_financeiro_id IS NOT NULL AND parcela_competencia_id IS NOT NULL AND parcela IS NOT NULL AND parcela > 0 AND competencia_int IS NOT NULL AND competencia_int BETWEEN 101 AND 999912 AND competencia_int % 100 BETWEEN 1 AND 12 AND data_vencimento IS NOT NULL AND premio_liquido >= 0 AND iof >= 0 AND premio_total = premio_liquido + iof AND premio_total IS NOT NULL AND premio_liquido IS NOT NULL AND iof IS NOT NULL AND (valor_pago IS NULL OR valor_pago = 0 OR valor_pago = premio_total) AND revisao_baixa >= 0)"));
        });
    }
}
