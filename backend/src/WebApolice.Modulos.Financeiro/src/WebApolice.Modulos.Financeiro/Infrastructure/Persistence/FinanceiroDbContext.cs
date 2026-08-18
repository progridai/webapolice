using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence;

public partial class FinanceiroDbContext : DbContext
{
    public FinanceiroDbContext()
    {
    }

    public FinanceiroDbContext(DbContextOptions<FinanceiroDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CobrancaAcompanhamento> CobrancaAcompanhamentos { get; set; }

    public virtual DbSet<ContaCobranca> ContaCobrancas { get; set; }

    public virtual DbSet<ConvenioCobranca> ConvenioCobrancas { get; set; }

    public virtual DbSet<EstipulanteFaturamentoConfig> EstipulanteFaturamentoConfigs { get; set; }

    public virtual DbSet<FormaPagamentoEstipulante> FormaPagamentoEstipulantes { get; set; }

    public virtual DbSet<FormaRetorno> FormaRetornos { get; set; }

    public virtual DbSet<FormaRetornoEstipulante> FormaRetornoEstipulantes { get; set; }

    public virtual DbSet<IdentificadorRemessaApi> IdentificadorRemessaApis { get; set; }

    public virtual DbSet<MovimentoCobrancaLog> MovimentoCobrancaLogs { get; set; }

    public virtual DbSet<RegraAgrupamentoFatura> RegraAgrupamentoFaturas { get; set; }

    public virtual DbSet<RetornoBancarioCodigo> RetornoBancarioCodigos { get; set; }

    public virtual DbSet<Titulo> Titulos { get; set; }

    public virtual DbSet<TituloPagamento> TituloPagamentos { get; set; }

    public virtual DbSet<TituloRetornoBancario> TituloRetornoBancarios { get; set; }

    public virtual DbSet<TituloStatus> TituloStatuses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=painel.bravida.com.br;Port=5432;Database=webapolice_teste;Username=bravito;Password=Bravida@2023!");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("unaccent");

        modelBuilder.Entity<CobrancaAcompanhamento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cobranca_acompanhamento_pkey");

            entity.ToTable("cobranca_acompanhamento", "financeiro");

            entity.HasIndex(e => e.ClienteId, "ix_cobranca_acompanhamento_cliente");

            entity.HasIndex(e => e.DataAcompanhamento, "ix_cobranca_acompanhamento_data");

            entity.HasIndex(e => e.LegadoId, "ux_cobranca_acompanhamento_legado").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.Contato)
                .HasMaxLength(150)
                .HasColumnName("contato");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DataAcompanhamento).HasColumnName("data_acompanhamento");
            entity.Property(e => e.Descricao).HasColumnName("descricao");
            entity.Property(e => e.HoraOriginal)
                .HasMaxLength(30)
                .HasColumnName("hora_original");
            entity.Property(e => e.LegadoClienteId).HasColumnName("legado_cliente_id");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.UsuarioLegadoId).HasColumnName("usuario_legado_id");
        });

        modelBuilder.Entity<ContaCobranca>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("conta_cobranca_pkey");

            entity.ToTable("conta_cobranca", "financeiro");

            entity.HasIndex(e => e.IdentificadorAgrupamento, "ix_conta_cobranca_agrupamento");

            entity.HasIndex(e => e.PessoaId, "ix_conta_cobranca_pessoa");

            entity.HasIndex(e => e.ClienteVinculoId, "ix_conta_cobranca_vinculo");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.ClienteVinculoId).HasColumnName("cliente_vinculo_id");
            entity.Property(e => e.ConvenioCobrancaId).HasColumnName("convenio_cobranca_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EstipulanteId).HasColumnName("estipulante_id");
            entity.Property(e => e.IdentificadorAgrupamento)
                .HasMaxLength(160)
                .HasColumnName("identificador_agrupamento");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.RegraAgrupamentoId).HasColumnName("regra_agrupamento_id");
            entity.Property(e => e.SubestipulanteId).HasColumnName("subestipulante_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ConvenioCobranca).WithMany(p => p.ContaCobrancas)
                .HasForeignKey(d => d.ConvenioCobrancaId)
                .HasConstraintName("conta_cobranca_convenio_cobranca_id_fkey");

            entity.HasOne(d => d.RegraAgrupamento).WithMany(p => p.ContaCobrancas)
                .HasForeignKey(d => d.RegraAgrupamentoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("conta_cobranca_regra_agrupamento_id_fkey");
        });

        modelBuilder.Entity<ConvenioCobranca>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("convenio_cobranca_pkey");

            entity.ToTable("convenio_cobranca", "financeiro");

            entity.HasIndex(e => e.BancoId, "ix_convenio_cobranca_banco");

            entity.HasIndex(e => e.LegadoId, "ux_convenio_cobranca_legado")
                .IsUnique()
                .HasFilter("(legado_id IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Agencia)
                .HasMaxLength(30)
                .HasColumnName("agencia");
            entity.Property(e => e.BancoId).HasColumnName("banco_id");
            entity.Property(e => e.CodigoEmpresa)
                .HasMaxLength(80)
                .HasColumnName("codigo_empresa");
            entity.Property(e => e.ComunicaVindi).HasColumnName("comunica_vindi");
            entity.Property(e => e.ContaCorrente)
                .HasMaxLength(30)
                .HasColumnName("conta_corrente");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EstBairro)
                .HasMaxLength(100)
                .HasColumnName("est_bairro");
            entity.Property(e => e.EstCep)
                .HasMaxLength(20)
                .HasColumnName("est_cep");
            entity.Property(e => e.EstCidade)
                .HasMaxLength(100)
                .HasColumnName("est_cidade");
            entity.Property(e => e.EstComplemento)
                .HasMaxLength(100)
                .HasColumnName("est_complemento");
            entity.Property(e => e.EstEndereco)
                .HasMaxLength(150)
                .HasColumnName("est_endereco");
            entity.Property(e => e.EstNome)
                .HasMaxLength(120)
                .HasColumnName("est_nome");
            entity.Property(e => e.EstNumero)
                .HasMaxLength(100)
                .HasColumnName("est_numero");
            entity.Property(e => e.EstUf)
                .HasMaxLength(4)
                .HasColumnName("est_uf");
            entity.Property(e => e.ExtensaoArquivo)
                .HasMaxLength(10)
                .HasColumnName("extensao_arquivo");
            entity.Property(e => e.InscricaoEstadual)
                .HasMaxLength(40)
                .HasColumnName("inscricao_estadual");
            entity.Property(e => e.LayoutArquivo).HasColumnName("layout_arquivo");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.LocalRemessaArquivo).HasColumnName("local_remessa_arquivo");
            entity.Property(e => e.LocalRetornoArquivo).HasColumnName("local_retorno_arquivo");
            entity.Property(e => e.Nome)
                .HasMaxLength(150)
                .HasColumnName("nome");
            entity.Property(e => e.NomeEmpresa)
                .HasMaxLength(150)
                .HasColumnName("nome_empresa");
            entity.Property(e => e.NomeInicialArquivo)
                .HasMaxLength(80)
                .HasColumnName("nome_inicial_arquivo");
            entity.Property(e => e.NumeroArquivo).HasColumnName("numero_arquivo");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<EstipulanteFaturamentoConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("estipulante_faturamento_config_pkey");

            entity.ToTable("estipulante_faturamento_config", "financeiro");

            entity.HasIndex(e => e.ConvenioCobrancaId, "ix_estipulante_faturamento_convenio");

            entity.HasIndex(e => e.FormaPagamentoId, "ix_estipulante_faturamento_forma");

            entity.HasIndex(e => e.EstipulanteId, "ux_estipulante_faturamento_config").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Campanha).HasColumnName("campanha");
            entity.Property(e => e.ConvenioCobrancaId).HasColumnName("convenio_cobranca_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DiaDebito).HasColumnName("dia_debito");
            entity.Property(e => e.EstipulanteId).HasColumnName("estipulante_id");
            entity.Property(e => e.FormaPagamentoId).HasColumnName("forma_pagamento_id");
            entity.Property(e => e.IofAp)
                .HasPrecision(10, 4)
                .HasColumnName("iof_ap");
            entity.Property(e => e.IofInc)
                .HasPrecision(10, 4)
                .HasColumnName("iof_inc");
            entity.Property(e => e.IofVg)
                .HasPrecision(10, 4)
                .HasColumnName("iof_vg");
            entity.Property(e => e.NumeroPropostaAp)
                .HasMaxLength(50)
                .HasColumnName("numero_proposta_ap");
            entity.Property(e => e.NumeroPropostaInc)
                .HasMaxLength(50)
                .HasColumnName("numero_proposta_inc");
            entity.Property(e => e.NumeroPropostaVg)
                .HasMaxLength(50)
                .HasColumnName("numero_proposta_vg");
            entity.Property(e => e.ParametroSiapeId).HasColumnName("parametro_siape_id");
            entity.Property(e => e.RegraAgrupamentoFaturaId).HasColumnName("regra_agrupamento_fatura_id");
            entity.Property(e => e.Saf)
                .HasMaxLength(80)
                .HasColumnName("saf");
            entity.Property(e => e.SorteioValor)
                .HasPrecision(18, 2)
                .HasColumnName("sorteio_valor");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ConvenioCobranca).WithMany(p => p.EstipulanteFaturamentoConfigs)
                .HasForeignKey(d => d.ConvenioCobrancaId)
                .HasConstraintName("estipulante_faturamento_config_convenio_cobranca_id_fkey");

            entity.HasOne(d => d.FormaPagamento).WithMany(p => p.EstipulanteFaturamentoConfigs)
                .HasForeignKey(d => d.FormaPagamentoId)
                .HasConstraintName("estipulante_faturamento_config_forma_pagamento_id_fkey");

            entity.HasOne(d => d.RegraAgrupamentoFatura).WithMany(p => p.EstipulanteFaturamentoConfigs)
                .HasForeignKey(d => d.RegraAgrupamentoFaturaId)
                .HasConstraintName("estipulante_faturamento_config_regra_agrupamento_fatura_id_fkey");
        });

        modelBuilder.Entity<FormaPagamentoEstipulante>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("forma_pagamento_estipulante_pkey");

            entity.ToTable("forma_pagamento_estipulante", "financeiro");

            entity.HasIndex(e => e.LegadoId, "ux_forma_pagamento_estipulante_legado")
                .IsUnique()
                .HasFilter("(legado_id IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.Codigo)
                .HasMaxLength(50)
                .HasColumnName("codigo");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");
        });

        modelBuilder.Entity<FormaRetorno>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("forma_retorno_pkey");

            entity.ToTable("forma_retorno", "financeiro");

            entity.HasIndex(e => e.LegadoId, "ux_forma_retorno_legado").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.Nome)
                .HasMaxLength(150)
                .HasColumnName("nome");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<FormaRetornoEstipulante>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("forma_retorno_estipulante_pkey");

            entity.ToTable("forma_retorno_estipulante", "financeiro");

            entity.HasIndex(e => e.EstipulanteId, "ix_forma_retorno_estipulante_estipulante");

            entity.HasIndex(e => e.FormaRetornoId, "ix_forma_retorno_estipulante_forma");

            entity.HasIndex(e => e.LegadoId, "ux_forma_retorno_estipulante_legado").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EstipulanteId).HasColumnName("estipulante_id");
            entity.Property(e => e.FormaRetornoId).HasColumnName("forma_retorno_id");
            entity.Property(e => e.LegadoEstipulanteId).HasColumnName("legado_estipulante_id");
            entity.Property(e => e.LegadoFormaRetornoId).HasColumnName("legado_forma_retorno_id");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");

            entity.HasOne(d => d.FormaRetorno).WithMany(p => p.FormaRetornoEstipulantes)
                .HasForeignKey(d => d.FormaRetornoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("forma_retorno_estipulante_forma_retorno_id_fkey");
        });

        modelBuilder.Entity<IdentificadorRemessaApi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("identificador_remessa_api_pkey");

            entity.ToTable("identificador_remessa_api", "financeiro");

            entity.HasIndex(e => e.Datahora, "ix_identificador_remessa_api_datahora");

            entity.HasIndex(e => e.LegadoId, "ux_identificador_remessa_api_legado").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Datahora).HasColumnName("datahora");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.UsuarioLegadoId).HasColumnName("usuario_legado_id");
        });

        modelBuilder.Entity<MovimentoCobrancaLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("movimento_cobranca_log_pkey");

            entity.ToTable("movimento_cobranca_log", "financeiro");

            entity.HasIndex(e => e.DataMovimento, "ix_movimento_cobranca_log_data");

            entity.HasIndex(e => e.LegadoMovimentoPropostaId, "ix_movimento_cobranca_log_legado_movimento");

            entity.HasIndex(e => e.PropostaMovimentoId, "ix_movimento_cobranca_log_movimento");

            entity.HasIndex(e => e.TituloId, "ix_movimento_cobranca_log_titulo");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DataAlteracao).HasColumnName("data_alteracao");
            entity.Property(e => e.DataMovimento).HasColumnName("data_movimento");
            entity.Property(e => e.DataPagamento).HasColumnName("data_pagamento");
            entity.Property(e => e.LegadoMovimentoPropostaId).HasColumnName("legado_movimento_proposta_id");
            entity.Property(e => e.PropostaMovimentoId).HasColumnName("proposta_movimento_id");
            entity.Property(e => e.TituloId).HasColumnName("titulo_id");
            entity.Property(e => e.UsuarioLegadoId).HasColumnName("usuario_legado_id");
            entity.Property(e => e.ValorPagamento)
                .HasPrecision(18, 2)
                .HasColumnName("valor_pagamento");

            entity.HasOne(d => d.Titulo).WithMany(p => p.MovimentoCobrancaLogs)
                .HasForeignKey(d => d.TituloId)
                .HasConstraintName("movimento_cobranca_log_titulo_id_fkey");
        });

        modelBuilder.Entity<RegraAgrupamentoFatura>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("regra_agrupamento_fatura_pkey");

            entity.ToTable("regra_agrupamento_fatura", "financeiro");

            entity.HasIndex(e => e.Codigo, "regra_agrupamento_fatura_codigo_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Codigo)
                .HasMaxLength(50)
                .HasColumnName("codigo");
            entity.Property(e => e.Descricao).HasColumnName("descricao");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");
        });

        modelBuilder.Entity<RetornoBancarioCodigo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("retorno_bancario_codigo_pkey");

            entity.ToTable("retorno_bancario_codigo", "financeiro");

            entity.HasIndex(e => e.Codigo, "ix_retorno_bancario_codigo_codigo");

            entity.HasIndex(e => e.Tipo, "ix_retorno_bancario_codigo_tipo");

            entity.HasIndex(e => new { e.Codigo, e.Descricao }, "retorno_bancario_codigo_codigo_descricao_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .HasColumnName("codigo");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Descricao)
                .HasMaxLength(200)
                .HasColumnName("descricao");
            entity.Property(e => e.GeraBaixa).HasColumnName("gera_baixa");
            entity.Property(e => e.GeraRejeicao).HasColumnName("gera_rejeicao");
            entity.Property(e => e.Tipo)
                .HasMaxLength(40)
                .HasDefaultValueSql("'indefinido'::character varying")
                .HasColumnName("tipo");
        });

        modelBuilder.Entity<Titulo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("titulo_pkey");

            entity.ToTable("titulo", "financeiro");

            entity.HasIndex(e => e.ClienteId, "ix_titulo_cliente");

            entity.HasIndex(e => new { e.CompetenciaAno, e.CompetenciaMes }, "ix_titulo_competencia");

            entity.HasIndex(e => e.CompetenciaInt, "ix_titulo_competencia_int");

            entity.HasIndex(e => e.EstipulanteId, "ix_titulo_estipulante");

            entity.HasIndex(e => e.DataPagamento, "ix_titulo_pagamento").HasFilter("(data_pagamento IS NOT NULL)");

            entity.HasIndex(e => e.PropostaId, "ix_titulo_proposta");

            entity.HasIndex(e => e.StatusId, "ix_titulo_status");

            entity.HasIndex(e => e.ClienteVinculoId, "ix_titulo_vinculo");

            entity.HasIndex(e => e.LegadoMovimentoPropostaId, "ux_titulo_legado_movimento")
                .IsUnique()
                .HasFilter("(legado_movimento_proposta_id IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.ClienteVinculoId).HasColumnName("cliente_vinculo_id");
            entity.Property(e => e.CobrarNaFatura).HasColumnName("cobrar_na_fatura");
            entity.Property(e => e.CompetenciaAno).HasColumnName("competencia_ano");
            entity.Property(e => e.CompetenciaInt).HasColumnName("competencia_int");
            entity.Property(e => e.CompetenciaMes).HasColumnName("competencia_mes");
            entity.Property(e => e.ContaCobrancaId).HasColumnName("conta_cobranca_id");
            entity.Property(e => e.ConvenioCobrancaId).HasColumnName("convenio_cobranca_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DataLancamento).HasColumnName("data_lancamento");
            entity.Property(e => e.DataPagamento).HasColumnName("data_pagamento");
            entity.Property(e => e.DataRecebimentoFatura).HasColumnName("data_recebimento_fatura");
            entity.Property(e => e.DataVencimento).HasColumnName("data_vencimento");
            entity.Property(e => e.DataVencimentoFatura).HasColumnName("data_vencimento_fatura");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.EstipulanteId).HasColumnName("estipulante_id");
            entity.Property(e => e.IdFaturaCartao)
                .HasMaxLength(100)
                .HasColumnName("id_fatura_cartao");
            entity.Property(e => e.Iof)
                .HasPrecision(18, 2)
                .HasColumnName("iof");
            entity.Property(e => e.LegadoMovimentoPropostaId).HasColumnName("legado_movimento_proposta_id");
            entity.Property(e => e.LegadoPropostaId).HasColumnName("legado_proposta_id");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.Parcela).HasColumnName("parcela");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.PremioAnterior)
                .HasPrecision(18, 2)
                .HasColumnName("premio_anterior");
            entity.Property(e => e.PremioAtual)
                .HasPrecision(18, 2)
                .HasColumnName("premio_atual");
            entity.Property(e => e.PremioDiferenca)
                .HasPrecision(18, 2)
                .HasColumnName("premio_diferenca");
            entity.Property(e => e.PremioFatura)
                .HasPrecision(18, 2)
                .HasColumnName("premio_fatura");
            entity.Property(e => e.PremioLiquido)
                .HasPrecision(18, 2)
                .HasColumnName("premio_liquido");
            entity.Property(e => e.PremioTotal)
                .HasPrecision(18, 2)
                .HasColumnName("premio_total");
            entity.Property(e => e.PremioTotalOriginal)
                .HasPrecision(18, 2)
                .HasColumnName("premio_total_original");
            entity.Property(e => e.PropostaId).HasColumnName("proposta_id");
            entity.Property(e => e.PropostaMovimentoId).HasColumnName("proposta_movimento_id");
            entity.Property(e => e.Sequencia).HasColumnName("sequencia");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.ValorAtual)
                .HasPrecision(18, 2)
                .HasColumnName("valor_atual");
            entity.Property(e => e.ValorOriginal)
                .HasPrecision(18, 2)
                .HasColumnName("valor_original");
            entity.Property(e => e.ValorPago)
                .HasPrecision(18, 2)
                .HasColumnName("valor_pago");

            entity.HasOne(d => d.ContaCobranca).WithMany(p => p.Titulos)
                .HasForeignKey(d => d.ContaCobrancaId)
                .HasConstraintName("titulo_conta_cobranca_id_fkey");

            entity.HasOne(d => d.ConvenioCobranca).WithMany(p => p.Titulos)
                .HasForeignKey(d => d.ConvenioCobrancaId)
                .HasConstraintName("titulo_convenio_cobranca_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.Titulos)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("titulo_status_id_fkey");
        });

        modelBuilder.Entity<TituloPagamento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("titulo_pagamento_pkey");

            entity.ToTable("titulo_pagamento", "financeiro");

            entity.HasIndex(e => e.DataPagamento, "ix_titulo_pagamento_data");

            entity.HasIndex(e => e.TituloId, "ix_titulo_pagamento_titulo");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DataPagamento).HasColumnName("data_pagamento");
            entity.Property(e => e.FormaPagamento)
                .HasMaxLength(50)
                .HasColumnName("forma_pagamento");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.Origem)
                .HasMaxLength(50)
                .HasDefaultValueSql("'legado'::character varying")
                .HasColumnName("origem");
            entity.Property(e => e.PropostaMovimentoId).HasColumnName("proposta_movimento_id");
            entity.Property(e => e.TituloId).HasColumnName("titulo_id");
            entity.Property(e => e.ValorPago)
                .HasPrecision(18, 2)
                .HasColumnName("valor_pago");

            entity.HasOne(d => d.Titulo).WithMany(p => p.TituloPagamentos)
                .HasForeignKey(d => d.TituloId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("titulo_pagamento_titulo_id_fkey");
        });

        modelBuilder.Entity<TituloRetornoBancario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("titulo_retorno_bancario_pkey");

            entity.ToTable("titulo_retorno_bancario", "financeiro");

            entity.HasIndex(e => e.RetornoCodigoId, "ix_titulo_retorno_codigo");

            entity.HasIndex(e => e.PropostaMovimentoId, "ix_titulo_retorno_movimento");

            entity.HasIndex(e => e.TituloId, "ix_titulo_retorno_titulo");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CodigoOriginal)
                .HasMaxLength(20)
                .HasColumnName("codigo_original");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DataRetorno).HasColumnName("data_retorno");
            entity.Property(e => e.DescricaoOriginal)
                .HasMaxLength(200)
                .HasColumnName("descricao_original");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.PropostaMovimentoId).HasColumnName("proposta_movimento_id");
            entity.Property(e => e.RetornoCodigoId).HasColumnName("retorno_codigo_id");
            entity.Property(e => e.TituloId).HasColumnName("titulo_id");

            entity.HasOne(d => d.RetornoCodigo).WithMany(p => p.TituloRetornoBancarios)
                .HasForeignKey(d => d.RetornoCodigoId)
                .HasConstraintName("titulo_retorno_bancario_retorno_codigo_id_fkey");

            entity.HasOne(d => d.Titulo).WithMany(p => p.TituloRetornoBancarios)
                .HasForeignKey(d => d.TituloId)
                .HasConstraintName("titulo_retorno_bancario_titulo_id_fkey");
        });

        modelBuilder.Entity<TituloStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("titulo_status_pkey");

            entity.ToTable("titulo_status", "financeiro");

            entity.HasIndex(e => e.Codigo, "titulo_status_codigo_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.Codigo)
                .HasMaxLength(40)
                .HasColumnName("codigo");
            entity.Property(e => e.Finalizador).HasColumnName("finalizador");
            entity.Property(e => e.Inadimplente).HasColumnName("inadimplente");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");
            entity.Property(e => e.PermiteCobranca)
                .HasDefaultValue(true)
                .HasColumnName("permite_cobranca");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
