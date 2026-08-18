using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Comissao.src.WebApolice.Modulos.Comissao.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Comissao.src.WebApolice.Modulos.Comissao.Infrastructure.Persistence;

public partial class ComissaoDbContext : DbContext
{
    public ComissaoDbContext()
    {
    }

    public ComissaoDbContext(DbContextOptions<ComissaoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AgenciadorComissaoConfig> AgenciadorComissaoConfigs { get; set; }

    public virtual DbSet<AgenciamentoCorretoraLancamento> AgenciamentoCorretoraLancamentos { get; set; }

    public virtual DbSet<CorretoraAgenciador> CorretoraAgenciadors { get; set; }

    public virtual DbSet<EstipulanteComissaoConfig> EstipulanteComissaoConfigs { get; set; }

    public virtual DbSet<FaturaComissaoResumo> FaturaComissaoResumos { get; set; }

    public virtual DbSet<FaturaIntegracao> FaturaIntegracaos { get; set; }

    public virtual DbSet<FaturaVidaAgenciamento> FaturaVidaAgenciamentos { get; set; }

    public virtual DbSet<FaturaVidaRecebimento> FaturaVidaRecebimentos { get; set; }

    public virtual DbSet<LancamentoComissao> LancamentoComissaos { get; set; }

    public virtual DbSet<LancamentoFaturaEstipulante> LancamentoFaturaEstipulantes { get; set; }

    public virtual DbSet<PropostaParticipante> PropostaParticipantes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=painel.bravida.com.br;Port=5432;Database=webapolice_teste;Username=bravito;Password=Bravida@2023!");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("unaccent");

        modelBuilder.Entity<AgenciadorComissaoConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("agenciador_comissao_config_pkey");

            entity.ToTable("agenciador_comissao_config", "comissao");

            entity.HasIndex(e => e.AgenciadorId, "ix_agenciador_comissao_config_agenciador");

            entity.HasIndex(e => new { e.InicioVigencia, e.FimVigencia }, "ix_agenciador_comissao_config_vigencia");

            entity.HasIndex(e => e.LegadoId, "ux_agenciador_comissao_config_legado")
                .IsUnique()
                .HasFilter("(legado_id IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AgenciadorId).HasColumnName("agenciador_id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.FimVigencia).HasColumnName("fim_vigencia");
            entity.Property(e => e.InicioVigencia).HasColumnName("inicio_vigencia");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.Origem)
                .HasMaxLength(80)
                .HasDefaultValueSql("'legado'::character varying")
                .HasColumnName("origem");
            entity.Property(e => e.PercentualPadrao)
                .HasPrecision(10, 4)
                .HasColumnName("percentual_padrao");
            entity.Property(e => e.PercentualRepasse)
                .HasPrecision(10, 4)
                .HasColumnName("percentual_repasse");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<AgenciamentoCorretoraLancamento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("agenciamento_corretora_lancamento_pkey");

            entity.ToTable("agenciamento_corretora_lancamento", "comissao");

            entity.HasIndex(e => e.CorretoraId, "ix_agenciamento_corretora_lancamento_corretora");

            entity.HasIndex(e => e.MovimentoTipoId, "ix_agenciamento_corretora_lancamento_movimento");

            entity.HasIndex(e => e.DataPagamento, "ix_agenciamento_corretora_lancamento_pagamento").HasFilter("(data_pagamento IS NOT NULL)");

            entity.HasIndex(e => e.PropostaId, "ix_agenciamento_corretora_lancamento_proposta");

            entity.HasIndex(e => e.DataVencimento, "ix_agenciamento_corretora_lancamento_vencimento");

            entity.HasIndex(e => e.LegadoId, "ux_agenciamento_corretora_lancamento_legado").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CorretoraId).HasColumnName("corretora_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DataCadastro).HasColumnName("data_cadastro");
            entity.Property(e => e.DataPagamento).HasColumnName("data_pagamento");
            entity.Property(e => e.DataVencimento).HasColumnName("data_vencimento");
            entity.Property(e => e.GerouFatura).HasColumnName("gerou_fatura");
            entity.Property(e => e.LegadoCorretoraId).HasColumnName("legado_corretora_id");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.LegadoMovimentoId).HasColumnName("legado_movimento_id");
            entity.Property(e => e.LegadoPropostaId).HasColumnName("legado_proposta_id");
            entity.Property(e => e.MovimentoTipoId).HasColumnName("movimento_tipo_id");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.ParcelaFinal).HasColumnName("parcela_final");
            entity.Property(e => e.ParcelaInicial).HasColumnName("parcela_inicial");
            entity.Property(e => e.Percentual)
                .HasPrecision(10, 4)
                .HasColumnName("percentual");
            entity.Property(e => e.PropostaId).HasColumnName("proposta_id");
            entity.Property(e => e.StatusLegado).HasColumnName("status_legado");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.ValorAgenciamento)
                .HasPrecision(18, 2)
                .HasColumnName("valor_agenciamento");
            entity.Property(e => e.ValorPago)
                .HasPrecision(18, 2)
                .HasColumnName("valor_pago");
            entity.Property(e => e.ValorPremio)
                .HasPrecision(18, 2)
                .HasColumnName("valor_premio");
        });

        modelBuilder.Entity<CorretoraAgenciador>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("corretora_agenciador_pkey");

            entity.ToTable("corretora_agenciador", "comissao");

            entity.HasIndex(e => e.AgenciadorId, "ix_corretora_agenciador_agenciador");

            entity.HasIndex(e => e.CorretoraId, "ix_corretora_agenciador_corretora");

            entity.HasIndex(e => e.LegadoId, "ux_corretora_agenciador_legado")
                .IsUnique()
                .HasFilter("(legado_id IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AgenciadorId).HasColumnName("agenciador_id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.CorretoraId).HasColumnName("corretora_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.FimVigencia).HasColumnName("fim_vigencia");
            entity.Property(e => e.InicioVigencia).HasColumnName("inicio_vigencia");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.PercentualAgenciamento)
                .HasPrecision(10, 4)
                .HasColumnName("percentual_agenciamento");
            entity.Property(e => e.PercentualRepasse)
                .HasPrecision(10, 4)
                .HasColumnName("percentual_repasse");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<EstipulanteComissaoConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("estipulante_comissao_config_pkey");

            entity.ToTable("estipulante_comissao_config", "comissao");

            entity.HasIndex(e => e.AgenciadorId, "ix_estipulante_comissao_agenciador");

            entity.HasIndex(e => e.EstipulanteId, "ux_estipulante_comissao_config").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AgenciadorId).HasColumnName("agenciador_id");
            entity.Property(e => e.AgenciadorPercentualRepasse)
                .HasPrecision(10, 4)
                .HasColumnName("agenciador_percentual_repasse");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.ComissaoApartirParcela).HasColumnName("comissao_apartir_parcela");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EstipulanteId).HasColumnName("estipulante_id");
            entity.Property(e => e.PercentualAgenciamento)
                .HasPrecision(10, 4)
                .HasColumnName("percentual_agenciamento");
            entity.Property(e => e.PercentualBonus)
                .HasPrecision(10, 4)
                .HasColumnName("percentual_bonus");
            entity.Property(e => e.PercentualComissao)
                .HasPrecision(10, 4)
                .HasColumnName("percentual_comissao");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<FaturaComissaoResumo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fatura_comissao_resumo_pkey");

            entity.ToTable("fatura_comissao_resumo", "comissao");

            entity.HasIndex(e => e.CompetenciaInt, "ix_fatura_comissao_resumo_competencia");

            entity.HasIndex(e => e.EstipulanteId, "ix_fatura_comissao_resumo_estipulante");

            entity.HasIndex(e => e.LegadoId, "ux_fatura_comissao_resumo_legado").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ano)
                .HasMaxLength(10)
                .HasColumnName("ano");
            entity.Property(e => e.CompetenciaInt).HasColumnName("competencia_int");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DataPagamento).HasColumnName("data_pagamento");
            entity.Property(e => e.EstipulanteId).HasColumnName("estipulante_id");
            entity.Property(e => e.LegadoEstipulanteId).HasColumnName("legado_estipulante_id");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.Mes)
                .HasMaxLength(10)
                .HasColumnName("mes");
            entity.Property(e => e.PremioPagamento)
                .HasPrecision(18, 2)
                .HasColumnName("premio_pagamento");
            entity.Property(e => e.ValorPago)
                .HasPrecision(18, 2)
                .HasColumnName("valor_pago");
        });

        modelBuilder.Entity<FaturaIntegracao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fatura_integracao_pkey");

            entity.ToTable("fatura_integracao", "comissao");

            entity.HasIndex(e => new { e.Ano, e.Mes }, "ix_fatura_integracao_competencia");

            entity.HasIndex(e => e.CorretoraId, "ix_fatura_integracao_corretora");

            entity.HasIndex(e => e.EstipulanteId, "ix_fatura_integracao_estipulante");

            entity.HasIndex(e => e.SeguradoraId, "ix_fatura_integracao_seguradora");

            entity.HasIndex(e => new { e.Tipo, e.SituacaoLegado }, "ix_fatura_integracao_tipo_situacao");

            entity.HasIndex(e => e.DataVencimento, "ix_fatura_integracao_vencimento");

            entity.HasIndex(e => e.LegadoId, "ux_fatura_integracao_legado").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Alterado).HasColumnName("alterado");
            entity.Property(e => e.Ano).HasColumnName("ano");
            entity.Property(e => e.CompetenciaInt).HasColumnName("competencia_int");
            entity.Property(e => e.CorretoraCodigoOriginal)
                .HasMaxLength(40)
                .HasColumnName("corretora_codigo_original");
            entity.Property(e => e.CorretoraId).HasColumnName("corretora_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DataLancamento).HasColumnName("data_lancamento");
            entity.Property(e => e.DataRecebimento).HasColumnName("data_recebimento");
            entity.Property(e => e.DataVencimento).HasColumnName("data_vencimento");
            entity.Property(e => e.EstipulanteId).HasColumnName("estipulante_id");
            entity.Property(e => e.GerouArquivo).HasColumnName("gerou_arquivo");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.Mes).HasColumnName("mes");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.PercentualAgenciamento)
                .HasPrecision(10, 4)
                .HasColumnName("percentual_agenciamento");
            entity.Property(e => e.PercentualCorretagem)
                .HasPrecision(10, 4)
                .HasColumnName("percentual_corretagem");
            entity.Property(e => e.SeguradoraCodigoOriginal)
                .HasMaxLength(40)
                .HasColumnName("seguradora_codigo_original");
            entity.Property(e => e.SeguradoraId).HasColumnName("seguradora_id");
            entity.Property(e => e.SituacaoLegado).HasColumnName("situacao_legado");
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .HasColumnName("tipo");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.ValorFatura)
                .HasPrecision(18, 2)
                .HasColumnName("valor_fatura");
            entity.Property(e => e.ValorReceber)
                .HasPrecision(18, 2)
                .HasColumnName("valor_receber");
            entity.Property(e => e.ValorRecebido)
                .HasPrecision(18, 2)
                .HasColumnName("valor_recebido");
        });

        modelBuilder.Entity<FaturaVidaAgenciamento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fatura_vida_agenciamento_pkey");

            entity.ToTable("fatura_vida_agenciamento", "comissao");

            entity.HasIndex(e => e.DataInclusao, "ix_fatura_vida_agenciamento_data_inclusao");

            entity.HasIndex(e => e.OrigemLegado, "ix_fatura_vida_agenciamento_origem");

            entity.HasIndex(e => e.PropostaId, "ix_fatura_vida_agenciamento_proposta");

            entity.HasIndex(e => new { e.OrigemLegado, e.LegadoId }, "ux_fatura_vida_agenciamento_legado").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CodigoCooperadoOriginal)
                .HasMaxLength(40)
                .HasColumnName("codigo_cooperado_original");
            entity.Property(e => e.CodigoCorretoraOriginal)
                .HasMaxLength(40)
                .HasColumnName("codigo_corretora_original");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DataInclusao).HasColumnName("data_inclusao");
            entity.Property(e => e.DataRegistro).HasColumnName("data_registro");
            entity.Property(e => e.Iof)
                .HasPrecision(18, 2)
                .HasColumnName("iof");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.LegadoPropostaId).HasColumnName("legado_proposta_id");
            entity.Property(e => e.NumeroNf)
                .HasMaxLength(120)
                .HasColumnName("numero_nf");
            entity.Property(e => e.OrigemLegado)
                .HasMaxLength(80)
                .HasColumnName("origem_legado");
            entity.Property(e => e.Premio)
                .HasPrecision(18, 2)
                .HasColumnName("premio");
            entity.Property(e => e.PremioLiquido)
                .HasPrecision(18, 2)
                .HasColumnName("premio_liquido");
            entity.Property(e => e.PropostaId).HasColumnName("proposta_id");
            entity.Property(e => e.TipoAgenciamento)
                .HasMaxLength(60)
                .HasColumnName("tipo_agenciamento");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.ValorAgenciamento)
                .HasPrecision(18, 2)
                .HasColumnName("valor_agenciamento");
            entity.Property(e => e.ValorDiferenca)
                .HasPrecision(18, 2)
                .HasColumnName("valor_diferenca");
            entity.Property(e => e.ValorRecebido)
                .HasPrecision(18, 2)
                .HasColumnName("valor_recebido");
        });

        modelBuilder.Entity<FaturaVidaRecebimento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fatura_vida_recebimento_pkey");

            entity.ToTable("fatura_vida_recebimento", "comissao");

            entity.HasIndex(e => e.DataPagamento, "ix_fatura_vida_recebimento_data");

            entity.HasIndex(e => e.EstipulanteId, "ix_fatura_vida_recebimento_estipulante");

            entity.HasIndex(e => e.FaturaVidaAgenciamentoId, "ix_fatura_vida_recebimento_fatura_vida");

            entity.HasIndex(e => e.LegadoId, "ux_fatura_vida_recebimento_legado").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DataPagamento).HasColumnName("data_pagamento");
            entity.Property(e => e.EstipulanteId).HasColumnName("estipulante_id");
            entity.Property(e => e.FaturaVidaAgenciamentoId).HasColumnName("fatura_vida_agenciamento_id");
            entity.Property(e => e.LegadoEstipulanteId).HasColumnName("legado_estipulante_id");
            entity.Property(e => e.LegadoFaturaVidaId).HasColumnName("legado_fatura_vida_id");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.Observacao)
                .HasMaxLength(150)
                .HasColumnName("observacao");
            entity.Property(e => e.Valor)
                .HasPrecision(18, 2)
                .HasColumnName("valor");

            entity.HasOne(d => d.FaturaVidaAgenciamento).WithMany(p => p.FaturaVidaRecebimentos)
                .HasForeignKey(d => d.FaturaVidaAgenciamentoId)
                .HasConstraintName("fatura_vida_recebimento_fatura_vida_agenciamento_id_fkey");
        });

        modelBuilder.Entity<LancamentoComissao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lancamento_comissao_pkey");

            entity.ToTable("lancamento_comissao", "comissao");

            entity.HasIndex(e => new { e.CompetenciaAno, e.CompetenciaMes }, "ix_lancamento_comissao_competencia");

            entity.HasIndex(e => e.PropostaMovimentoId, "ix_lancamento_comissao_movimento");

            entity.HasIndex(e => e.PropostaId, "ix_lancamento_comissao_proposta");

            entity.HasIndex(e => e.TituloId, "ix_lancamento_comissao_titulo");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.CompetenciaAno).HasColumnName("competencia_ano");
            entity.Property(e => e.CompetenciaInt).HasColumnName("competencia_int");
            entity.Property(e => e.CompetenciaMes).HasColumnName("competencia_mes");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EstipulanteId).HasColumnName("estipulante_id");
            entity.Property(e => e.Gerado)
                .HasMaxLength(1)
                .HasColumnName("gerado");
            entity.Property(e => e.LegadoMovimentoPropostaId).HasColumnName("legado_movimento_proposta_id");
            entity.Property(e => e.Origem)
                .HasMaxLength(50)
                .HasDefaultValueSql("'movimento_proposta_legado'::character varying")
                .HasColumnName("origem");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.PropostaId).HasColumnName("proposta_id");
            entity.Property(e => e.PropostaMovimentoId).HasColumnName("proposta_movimento_id");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValueSql("'pendente'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TituloId).HasColumnName("titulo_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.ValorBase)
                .HasPrecision(18, 2)
                .HasColumnName("valor_base");
            entity.Property(e => e.ValorBruto)
                .HasPrecision(18, 2)
                .HasColumnName("valor_bruto");
            entity.Property(e => e.ValorLiquido)
                .HasPrecision(18, 2)
                .HasColumnName("valor_liquido");
        });

        modelBuilder.Entity<LancamentoFaturaEstipulante>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lancamento_fatura_estipulante_pkey");

            entity.ToTable("lancamento_fatura_estipulante", "comissao");

            entity.HasIndex(e => e.CompetenciaInt, "ix_lancamento_fatura_estipulante_competencia");

            entity.HasIndex(e => e.CorretoraId, "ix_lancamento_fatura_estipulante_corretora");

            entity.HasIndex(e => e.EstipulanteId, "ix_lancamento_fatura_estipulante_estipulante");

            entity.HasIndex(e => e.LegadoId, "ux_lancamento_fatura_estipulante_legado").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ComissaoRecebida)
                .HasPrecision(18, 2)
                .HasColumnName("comissao_recebida");
            entity.Property(e => e.CompetenciaAno).HasColumnName("competencia_ano");
            entity.Property(e => e.CompetenciaInt).HasColumnName("competencia_int");
            entity.Property(e => e.CompetenciaMes).HasColumnName("competencia_mes");
            entity.Property(e => e.CompetenciaOriginal)
                .HasMaxLength(40)
                .HasColumnName("competencia_original");
            entity.Property(e => e.CorretoraId).HasColumnName("corretora_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DataRecebimento).HasColumnName("data_recebimento");
            entity.Property(e => e.DataVencimentoFatura).HasColumnName("data_vencimento_fatura");
            entity.Property(e => e.EstipulanteId).HasColumnName("estipulante_id");
            entity.Property(e => e.LancamentoManual).HasColumnName("lancamento_manual");
            entity.Property(e => e.LegadoCorretoraId).HasColumnName("legado_corretora_id");
            entity.Property(e => e.LegadoEstipulanteId).HasColumnName("legado_estipulante_id");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.PercentualCorretagem)
                .HasPrecision(10, 4)
                .HasColumnName("percentual_corretagem");
            entity.Property(e => e.PremioTotal)
                .HasPrecision(18, 2)
                .HasColumnName("premio_total");
            entity.Property(e => e.ValorFaturado)
                .HasPrecision(18, 2)
                .HasColumnName("valor_faturado");
        });

        modelBuilder.Entity<PropostaParticipante>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("proposta_participante_pkey");

            entity.ToTable("proposta_participante", "comissao");

            entity.HasIndex(e => e.AgenciadorId, "ix_proposta_participante_agenciador");

            entity.HasIndex(e => e.CodigoLegadoParticipante, "ix_proposta_participante_codigo_legado");

            entity.HasIndex(e => e.CorretoraId, "ix_proposta_participante_corretora");

            entity.HasIndex(e => e.PropostaId, "ix_proposta_participante_proposta");

            entity.HasIndex(e => e.ParticipanteTipo, "ix_proposta_participante_tipo");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AgenciadorId).HasColumnName("agenciador_id");
            entity.Property(e => e.AgenciamentoParcelaFinal).HasColumnName("agenciamento_parcela_final");
            entity.Property(e => e.AgenciamentoParcelaInicial).HasColumnName("agenciamento_parcela_inicial");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.Bonus)
                .HasPrecision(18, 2)
                .HasColumnName("bonus");
            entity.Property(e => e.CarteiraParcelaInicial).HasColumnName("carteira_parcela_inicial");
            entity.Property(e => e.CodigoAgenciamento)
                .HasMaxLength(80)
                .HasColumnName("codigo_agenciamento");
            entity.Property(e => e.CodigoLegadoParticipante).HasColumnName("codigo_legado_participante");
            entity.Property(e => e.CorretoraId).HasColumnName("corretora_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.LegadoCampoOrigem)
                .HasMaxLength(80)
                .HasColumnName("legado_campo_origem");
            entity.Property(e => e.ParticipanteId).HasColumnName("participante_id");
            entity.Property(e => e.ParticipanteTipo)
                .HasMaxLength(40)
                .HasColumnName("participante_tipo");
            entity.Property(e => e.PercentualAgenciamento)
                .HasPrecision(18, 4)
                .HasColumnName("percentual_agenciamento");
            entity.Property(e => e.PercentualCarteira)
                .HasPrecision(18, 4)
                .HasColumnName("percentual_carteira");
            entity.Property(e => e.PropostaId).HasColumnName("proposta_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
