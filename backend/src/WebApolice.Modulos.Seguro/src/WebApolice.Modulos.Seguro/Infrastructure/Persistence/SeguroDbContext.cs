using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

public partial class SeguroDbContext : DbContext
{
    public SeguroDbContext()
    {
    }

    public SeguroDbContext(DbContextOptions<SeguroDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cobertura> Coberturas { get; set; }

    public virtual DbSet<MovimentoTipo> MovimentoTipos { get; set; }

    public virtual DbSet<Plano> Planos { get; set; }

    public virtual DbSet<Produto> Produtos { get; set; }

    public virtual DbSet<PropostaBeneficiario> PropostaBeneficiarios { get; set; }

    public virtual DbSet<PropostaCobertura> PropostaCoberturas { get; set; }

    public virtual DbSet<PropostaHistorico> PropostaHistoricos { get; set; }

    public virtual DbSet<PropostaItem> PropostaItems { get; set; }

    public virtual DbSet<PropostaMovimento> PropostaMovimentos { get; set; }

    public virtual DbSet<PropostaStatus> PropostaStatuses { get; set; }

    public virtual DbSet<Propostum> Proposta { get; set; }

    public virtual DbSet<TabelaPreco> TabelaPrecos { get; set; }

    public virtual DbSet<TipoProduto> TipoProdutos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=painel.bravida.com.br;Port=5432;Database=webapolice_teste;Username=bravito;Password=Bravida@2023!");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("unaccent");

        modelBuilder.Entity<Cobertura>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cobertura_pkey");

            entity.ToTable("cobertura", "seguro");

            entity.HasIndex(e => e.Nome, "ix_cobertura_nome_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.LegadoId, "ux_cobertura_legado")
                .IsUnique()
                .HasFilter("(legado_id IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.Basica)
                .HasMaxLength(50)
                .HasColumnName("basica");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.LegadoCoberturaAnt).HasColumnName("legado_cobertura_ant");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.Nome)
                .HasMaxLength(150)
                .HasColumnName("nome");
            entity.Property(e => e.NomeReduzido)
                .HasMaxLength(30)
                .HasColumnName("nome_reduzido");
            entity.Property(e => e.Reajuste).HasColumnName("reajuste");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<MovimentoTipo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("movimento_tipo_pkey");

            entity.ToTable("movimento_tipo", "seguro");

            entity.HasIndex(e => e.Classificacao, "ix_movimento_tipo_classificacao");

            entity.HasIndex(e => e.Financeiro, "ix_movimento_tipo_financeiro");

            entity.HasIndex(e => e.LegadoId, "ux_movimento_tipo_legado")
                .IsUnique()
                .HasFilter("(legado_id IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AlteraProposta).HasColumnName("altera_proposta");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.Cancelamento).HasColumnName("cancelamento");
            entity.Property(e => e.Classificacao)
                .HasMaxLength(40)
                .HasDefaultValueSql("'avaliar'::character varying")
                .HasColumnName("classificacao");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Financeiro).HasColumnName("financeiro");
            entity.Property(e => e.GeraTitulo).HasColumnName("gera_titulo");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.Nome)
                .HasMaxLength(120)
                .HasColumnName("nome");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Plano>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("plano_pkey");

            entity.ToTable("plano", "seguro");

            entity.HasIndex(e => e.Nome, "ix_plano_nome_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.Ramo, "ix_plano_ramo");

            entity.HasIndex(e => e.LegadoId, "ux_plano_legado")
                .IsUnique()
                .HasFilter("(legado_id IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.LegadoPlanoAnt).HasColumnName("legado_plano_ant");
            entity.Property(e => e.Nome)
                .HasMaxLength(150)
                .HasColumnName("nome");
            entity.Property(e => e.Paga).HasColumnName("paga");
            entity.Property(e => e.Ramo)
                .HasMaxLength(80)
                .HasColumnName("ramo");
            entity.Property(e => e.Reajuste).HasColumnName("reajuste");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("produto_pkey");

            entity.ToTable("produto", "seguro");

            entity.HasIndex(e => e.CodigoReferencia, "ix_produto_codigo_referencia");

            entity.HasIndex(e => e.PlanoId, "ix_produto_plano");

            entity.HasIndex(e => e.TabelaPrecoId, "ix_produto_tabela_preco");

            entity.HasIndex(e => e.LegadoId, "ux_produto_legado")
                .IsUnique()
                .HasFilter("(legado_id IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.CodigoReferencia)
                .HasMaxLength(80)
                .HasColumnName("codigo_referencia");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.GeraConjuge).HasColumnName("gera_conjuge");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.LegadoProdutoAnt).HasColumnName("legado_produto_ant");
            entity.Property(e => e.Nome)
                .HasMaxLength(150)
                .HasColumnName("nome");
            entity.Property(e => e.PagaComissao).HasColumnName("paga_comissao");
            entity.Property(e => e.PlanoId).HasColumnName("plano_id");
            entity.Property(e => e.Ramo)
                .HasMaxLength(80)
                .HasColumnName("ramo");
            entity.Property(e => e.TabelaPrecoId).HasColumnName("tabela_preco_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Plano).WithMany(p => p.Produtos)
                .HasForeignKey(d => d.PlanoId)
                .HasConstraintName("produto_plano_id_fkey");

            entity.HasOne(d => d.TabelaPreco).WithMany(p => p.Produtos)
                .HasForeignKey(d => d.TabelaPrecoId)
                .HasConstraintName("produto_tabela_preco_id_fkey");
        });

        modelBuilder.Entity<PropostaBeneficiario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("proposta_beneficiario_pkey");

            entity.ToTable("proposta_beneficiario", "seguro");

            entity.HasIndex(e => e.CpfLimpo, "ix_proposta_beneficiario_cpf");

            entity.HasIndex(e => e.Nome, "ix_proposta_beneficiario_nome_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.ParentescoNormalizado, "ix_proposta_beneficiario_parentesco");

            entity.HasIndex(e => e.PessoaId, "ix_proposta_beneficiario_pessoa");

            entity.HasIndex(e => e.PropostaId, "ix_proposta_beneficiario_proposta");

            entity.HasIndex(e => e.LegadoId, "ux_proposta_beneficiario_legado").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CpfLimpo)
                .HasMaxLength(20)
                .HasColumnName("cpf_limpo");
            entity.Property(e => e.CpfOriginal)
                .HasMaxLength(50)
                .HasColumnName("cpf_original");
            entity.Property(e => e.CpfValido).HasColumnName("cpf_valido");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.Nome)
                .HasMaxLength(150)
                .HasColumnName("nome");
            entity.Property(e => e.NomeNormalizado)
                .HasMaxLength(150)
                .HasColumnName("nome_normalizado");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.Ordem).HasColumnName("ordem");
            entity.Property(e => e.ParentescoNormalizado)
                .HasMaxLength(60)
                .HasColumnName("parentesco_normalizado");
            entity.Property(e => e.ParentescoOriginal)
                .HasMaxLength(100)
                .HasColumnName("parentesco_original");
            entity.Property(e => e.PercentualParticipacao)
                .HasPrecision(10, 4)
                .HasColumnName("percentual_participacao");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.PropostaId).HasColumnName("proposta_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Proposta).WithMany(p => p.PropostaBeneficiarios)
                .HasForeignKey(d => d.PropostaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("proposta_beneficiario_proposta_id_fkey");
        });

        modelBuilder.Entity<PropostaCobertura>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("proposta_cobertura_pkey");

            entity.ToTable("proposta_cobertura", "seguro");

            entity.HasIndex(e => e.CoberturaId, "ix_proposta_cobertura_cobertura");

            entity.HasIndex(e => e.PropostaItemId, "ix_proposta_cobertura_item");

            entity.HasIndex(e => e.PropostaId, "ix_proposta_cobertura_proposta");

            entity.HasIndex(e => e.LegadoId, "ux_proposta_cobertura_legado").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.Basica).HasColumnName("basica");
            entity.Property(e => e.CoberturaId).HasColumnName("cobertura_id");
            entity.Property(e => e.CoberturaNomeLegado)
                .HasMaxLength(150)
                .HasColumnName("cobertura_nome_legado");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.LegadoPropostaCoberturaAnt).HasColumnName("legado_proposta_cobertura_ant");
            entity.Property(e => e.PremioConjuge)
                .HasPrecision(18, 2)
                .HasColumnName("premio_conjuge");
            entity.Property(e => e.PremioTitular)
                .HasPrecision(18, 2)
                .HasColumnName("premio_titular");
            entity.Property(e => e.PropostaId).HasColumnName("proposta_id");
            entity.Property(e => e.PropostaItemId).HasColumnName("proposta_item_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Cobertura).WithMany(p => p.PropostaCoberturas)
                .HasForeignKey(d => d.CoberturaId)
                .HasConstraintName("proposta_cobertura_cobertura_id_fkey");

            entity.HasOne(d => d.Proposta).WithMany(p => p.PropostaCoberturas)
                .HasForeignKey(d => d.PropostaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("proposta_cobertura_proposta_id_fkey");

            entity.HasOne(d => d.PropostaItem).WithMany(p => p.PropostaCoberturas)
                .HasForeignKey(d => d.PropostaItemId)
                .HasConstraintName("proposta_cobertura_proposta_item_id_fkey");
        });

        modelBuilder.Entity<PropostaHistorico>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("proposta_historico_pkey");

            entity.ToTable("proposta_historico", "seguro");

            entity.HasIndex(e => e.PropostaAnteriorId, "ix_proposta_historico_anterior");

            entity.HasIndex(e => e.PropostaNovaId, "ix_proposta_historico_nova");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DataAlteracao)
                .HasDefaultValueSql("now()")
                .HasColumnName("data_alteracao");
            entity.Property(e => e.LegadoOrigem)
                .HasMaxLength(80)
                .HasColumnName("legado_origem");
            entity.Property(e => e.Motivo)
                .HasMaxLength(150)
                .HasColumnName("motivo");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.PropostaAnteriorId).HasColumnName("proposta_anterior_id");
            entity.Property(e => e.PropostaNovaId).HasColumnName("proposta_nova_id");

            entity.HasOne(d => d.PropostaAnterior).WithMany(p => p.PropostaHistoricoPropostaAnteriors)
                .HasForeignKey(d => d.PropostaAnteriorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("proposta_historico_proposta_anterior_id_fkey");

            entity.HasOne(d => d.PropostaNova).WithMany(p => p.PropostaHistoricoPropostaNovas)
                .HasForeignKey(d => d.PropostaNovaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("proposta_historico_proposta_nova_id_fkey");
        });

        modelBuilder.Entity<PropostaItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("proposta_item_pkey");

            entity.ToTable("proposta_item", "seguro");

            entity.HasIndex(e => e.PlanoId, "ix_proposta_item_plano");

            entity.HasIndex(e => e.ProdutoId, "ix_proposta_item_produto");

            entity.HasIndex(e => e.PropostaId, "ix_proposta_item_proposta");

            entity.HasIndex(e => e.TabelaPrecoId, "ix_proposta_item_tabela");

            entity.HasIndex(e => e.TipoProdutoId, "ix_proposta_item_tipo");

            entity.HasIndex(e => e.LegadoId, "ux_proposta_item_legado").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.CdMovVid).HasColumnName("cd_mov_vid");
            entity.Property(e => e.CodigoLegado).HasColumnName("codigo_legado");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.LegadoPropostaTipoAnt).HasColumnName("legado_proposta_tipo_ant");
            entity.Property(e => e.PagaComissao).HasColumnName("paga_comissao");
            entity.Property(e => e.PlanoCodigoLegado)
                .HasMaxLength(100)
                .HasColumnName("plano_codigo_legado");
            entity.Property(e => e.PlanoId).HasColumnName("plano_id");
            entity.Property(e => e.PlanoNomeLegado)
                .HasMaxLength(150)
                .HasColumnName("plano_nome_legado");
            entity.Property(e => e.ProdutoId).HasColumnName("produto_id");
            entity.Property(e => e.PropostaId).HasColumnName("proposta_id");
            entity.Property(e => e.Ramo)
                .HasMaxLength(100)
                .HasColumnName("ramo");
            entity.Property(e => e.TabelaPrecoId).HasColumnName("tabela_preco_id");
            entity.Property(e => e.TipoProdutoId).HasColumnName("tipo_produto_id");
            entity.Property(e => e.UltimaFaixaEtaria).HasColumnName("ultima_faixa_etaria");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.Valor)
                .HasPrecision(18, 2)
                .HasColumnName("valor");

            entity.HasOne(d => d.Plano).WithMany(p => p.PropostaItems)
                .HasForeignKey(d => d.PlanoId)
                .HasConstraintName("proposta_item_plano_id_fkey");

            entity.HasOne(d => d.Produto).WithMany(p => p.PropostaItems)
                .HasForeignKey(d => d.ProdutoId)
                .HasConstraintName("proposta_item_produto_id_fkey");

            entity.HasOne(d => d.Proposta).WithMany(p => p.PropostaItems)
                .HasForeignKey(d => d.PropostaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("proposta_item_proposta_id_fkey");

            entity.HasOne(d => d.TabelaPreco).WithMany(p => p.PropostaItems)
                .HasForeignKey(d => d.TabelaPrecoId)
                .HasConstraintName("proposta_item_tabela_preco_id_fkey");

            entity.HasOne(d => d.TipoProduto).WithMany(p => p.PropostaItems)
                .HasForeignKey(d => d.TipoProdutoId)
                .HasConstraintName("proposta_item_tipo_produto_id_fkey");
        });

        modelBuilder.Entity<PropostaMovimento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("proposta_movimento_pkey");

            entity.ToTable("proposta_movimento", "seguro");

            entity.HasIndex(e => e.Classificacao, "ix_proposta_movimento_classificacao");

            entity.HasIndex(e => e.ClienteId, "ix_proposta_movimento_cliente");

            entity.HasIndex(e => new { e.Ano, e.Mes }, "ix_proposta_movimento_competencia");

            entity.HasIndex(e => e.CompetenciaInt, "ix_proposta_movimento_competencia_int");

            entity.HasIndex(e => e.DataPagamento, "ix_proposta_movimento_data_pagamento").HasFilter("(data_pagamento IS NOT NULL)");

            entity.HasIndex(e => e.EstipulanteId, "ix_proposta_movimento_estipulante");

            entity.HasIndex(e => e.PropostaId, "ix_proposta_movimento_proposta");

            entity.HasIndex(e => e.MovimentoTipoId, "ix_proposta_movimento_tipo");

            entity.HasIndex(e => e.ClienteVinculoId, "ix_proposta_movimento_vinculo");

            entity.HasIndex(e => e.LegadoId, "ux_proposta_movimento_legado").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ano).HasColumnName("ano");
            entity.Property(e => e.Classificacao)
                .HasMaxLength(40)
                .HasDefaultValueSql("'avaliar'::character varying")
                .HasColumnName("classificacao");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.ClienteVinculoId).HasColumnName("cliente_vinculo_id");
            entity.Property(e => e.CobrarNaFatura).HasColumnName("cobrar_na_fatura");
            entity.Property(e => e.ComissaoBase)
                .HasPrecision(18, 2)
                .HasColumnName("comissao_base");
            entity.Property(e => e.ComissaoBruta)
                .HasPrecision(18, 2)
                .HasColumnName("comissao_bruta");
            entity.Property(e => e.ComissaoGerado)
                .HasMaxLength(1)
                .HasColumnName("comissao_gerado");
            entity.Property(e => e.ComissaoLiquida)
                .HasPrecision(18, 2)
                .HasColumnName("comissao_liquida");
            entity.Property(e => e.CompetenciaInt).HasColumnName("competencia_int");
            entity.Property(e => e.ConvenioCobrancaId).HasColumnName("convenio_cobranca_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DataLancamento).HasColumnName("data_lancamento");
            entity.Property(e => e.DataPagamento).HasColumnName("data_pagamento");
            entity.Property(e => e.DataRecebimentoFatura).HasColumnName("data_recebimento_fatura");
            entity.Property(e => e.DataVencimento).HasColumnName("data_vencimento");
            entity.Property(e => e.DataVencimentoFatura).HasColumnName("data_vencimento_fatura");
            entity.Property(e => e.Dia).HasColumnName("dia");
            entity.Property(e => e.EstipulanteId).HasColumnName("estipulante_id");
            entity.Property(e => e.Gerado)
                .HasMaxLength(1)
                .HasColumnName("gerado");
            entity.Property(e => e.IdFaturaCartao)
                .HasMaxLength(100)
                .HasColumnName("id_fatura_cartao");
            entity.Property(e => e.Iof)
                .HasPrecision(18, 2)
                .HasColumnName("iof");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.LegadoMovAnt).HasColumnName("legado_mov_ant");
            entity.Property(e => e.Mes).HasColumnName("mes");
            entity.Property(e => e.MovimentoTipoId).HasColumnName("movimento_tipo_id");
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
            entity.Property(e => e.Sequencia).HasColumnName("sequencia");
            entity.Property(e => e.SituacaoCodigo).HasColumnName("situacao_codigo");
            entity.Property(e => e.SituacaoDescricao)
                .HasMaxLength(200)
                .HasColumnName("situacao_descricao");
            entity.Property(e => e.TituloGerado)
                .HasMaxLength(1)
                .HasColumnName("titulo_gerado");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UsuarioCobradorLegadoId).HasColumnName("usuario_cobrador_legado_id");
            entity.Property(e => e.ValorPago)
                .HasPrecision(18, 2)
                .HasColumnName("valor_pago");

            entity.HasOne(d => d.MovimentoTipo).WithMany(p => p.PropostaMovimentos)
                .HasForeignKey(d => d.MovimentoTipoId)
                .HasConstraintName("proposta_movimento_movimento_tipo_id_fkey");

            entity.HasOne(d => d.Proposta).WithMany(p => p.PropostaMovimentos)
                .HasForeignKey(d => d.PropostaId)
                .HasConstraintName("proposta_movimento_proposta_id_fkey");
        });

        modelBuilder.Entity<PropostaStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("proposta_status_pkey");

            entity.ToTable("proposta_status", "seguro");

            entity.HasIndex(e => e.Codigo, "proposta_status_codigo_key").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Codigo)
                .HasMaxLength(30)
                .HasColumnName("codigo");
            entity.Property(e => e.Finalizador).HasColumnName("finalizador");
            entity.Property(e => e.Nome)
                .HasMaxLength(80)
                .HasColumnName("nome");
            entity.Property(e => e.PermiteMovimentacao)
                .HasDefaultValue(true)
                .HasColumnName("permite_movimentacao");
            entity.Property(e => e.VisivelOperacional)
                .HasDefaultValue(true)
                .HasColumnName("visivel_operacional");
        });

        modelBuilder.Entity<Propostum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("proposta_pkey");

            entity.ToTable("proposta", "seguro");

            entity.HasIndex(e => e.ClienteId, "ix_proposta_cliente");

            entity.HasIndex(e => e.ClienteVinculoId, "ix_proposta_cliente_vinculo");

            entity.HasIndex(e => e.DataInclusao, "ix_proposta_data_inclusao");

            entity.HasIndex(e => e.EstipulanteId, "ix_proposta_estipulante");

            entity.HasIndex(e => new { e.EstipulanteId, e.StatusId }, "ix_proposta_estipulante_status");

            entity.HasIndex(e => e.Numero, "ix_proposta_numero");

            entity.HasIndex(e => e.PessoaId, "ix_proposta_pessoa");

            entity.HasIndex(e => e.StatusId, "ix_proposta_status");

            entity.HasIndex(e => e.Vigente, "ix_proposta_vigente");

            entity.HasIndex(e => e.LegadoId, "ux_proposta_legado").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BancoAgencia)
                .HasMaxLength(30)
                .HasColumnName("banco_agencia");
            entity.Property(e => e.BancoContaCorrente)
                .HasMaxLength(30)
                .HasColumnName("banco_conta_corrente");
            entity.Property(e => e.BancoDataDebito).HasColumnName("banco_data_debito");
            entity.Property(e => e.BancoDiaDebito)
                .HasMaxLength(10)
                .HasColumnName("banco_dia_debito");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.ClienteVinculoId).HasColumnName("cliente_vinculo_id");
            entity.Property(e => e.ComissaoEstornada).HasColumnName("comissao_estornada");
            entity.Property(e => e.CompetenciaInclusaoInt).HasColumnName("competencia_inclusao_int");
            entity.Property(e => e.ContaCobrancaId).HasColumnName("conta_cobranca_id");
            entity.Property(e => e.ConvenioCobrancaId).HasColumnName("convenio_cobranca_id");
            entity.Property(e => e.CorretoraId).HasColumnName("corretora_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DataAlteracaoSituacao).HasColumnName("data_alteracao_situacao");
            entity.Property(e => e.DataEstornoComissao).HasColumnName("data_estorno_comissao");
            entity.Property(e => e.DataInclusao).HasColumnName("data_inclusao");
            entity.Property(e => e.DataMovimento).HasColumnName("data_movimento");
            entity.Property(e => e.DataPrimeiroVencimento).HasColumnName("data_primeiro_vencimento");
            entity.Property(e => e.DataProcessamentoFunpresp).HasColumnName("data_processamento_funpresp");
            entity.Property(e => e.DataProximoVencimento).HasColumnName("data_proximo_vencimento");
            entity.Property(e => e.DataUltimoAjusteIndice).HasColumnName("data_ultimo_ajuste_indice");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.EstipulanteId).HasColumnName("estipulante_id");
            entity.Property(e => e.IofPercentual)
                .HasPrecision(18, 4)
                .HasColumnName("iof_percentual");
            entity.Property(e => e.IofValor)
                .HasPrecision(18, 2)
                .HasColumnName("iof_valor");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.LegadoMovimentoFim)
                .HasMaxLength(50)
                .HasColumnName("legado_movimento_fim");
            entity.Property(e => e.LegadoMovimentoIni)
                .HasMaxLength(50)
                .HasColumnName("legado_movimento_ini");
            entity.Property(e => e.LegadoPropostaAnt).HasColumnName("legado_proposta_ant");
            entity.Property(e => e.LotacaoId).HasColumnName("lotacao_id");
            entity.Property(e => e.MovimentoFaturaAno).HasColumnName("movimento_fatura_ano");
            entity.Property(e => e.MovimentoFaturaMes).HasColumnName("movimento_fatura_mes");
            entity.Property(e => e.MovimentoTipoId).HasColumnName("movimento_tipo_id");
            entity.Property(e => e.Numero)
                .HasMaxLength(100)
                .HasColumnName("numero");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.PossuiBonusFunpresp).HasColumnName("possui_bonus_funpresp");
            entity.Property(e => e.PremioLiquido)
                .HasPrecision(18, 2)
                .HasColumnName("premio_liquido");
            entity.Property(e => e.PropostaOrigemId).HasColumnName("proposta_origem_id");
            entity.Property(e => e.ProtocoloClienteLegadoId).HasColumnName("protocolo_cliente_legado_id");
            entity.Property(e => e.ProtocoloStatus).HasColumnName("protocolo_status");
            entity.Property(e => e.PublicId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("public_id");
            entity.Property(e => e.SeguradoraId).HasColumnName("seguradora_id");
            entity.Property(e => e.SituacaoProposta).HasColumnName("situacao_proposta");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.SubestipulanteId).HasColumnName("subestipulante_id");
            entity.Property(e => e.SubgrupoId).HasColumnName("subgrupo_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.ValorParcela)
                .HasPrecision(18, 2)
                .HasColumnName("valor_parcela");
            entity.Property(e => e.Versao)
                .HasDefaultValue(1)
                .HasColumnName("versao");
            entity.Property(e => e.Vigente).HasColumnName("vigente");
            entity.Property(e => e.VisivelOperacional)
                .HasDefaultValue(true)
                .HasColumnName("visivel_operacional");

            entity.HasOne(d => d.PropostaOrigem).WithMany(p => p.InversePropostaOrigem)
                .HasForeignKey(d => d.PropostaOrigemId)
                .HasConstraintName("proposta_proposta_origem_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.Proposta)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("proposta_status_id_fkey");
        });

        modelBuilder.Entity<TabelaPreco>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tabela_preco_pkey");

            entity.ToTable("tabela_preco", "seguro");

            entity.HasIndex(e => e.LegadoId, "ux_tabela_preco_legado")
                .IsUnique()
                .HasFilter("(legado_id IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.Codigo)
                .HasMaxLength(80)
                .HasColumnName("codigo");
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

        modelBuilder.Entity<TipoProduto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tipo_produto_pkey");

            entity.ToTable("tipo_produto", "seguro");

            entity.HasIndex(e => e.Nome, "ix_tipo_produto_nome");

            entity.HasIndex(e => e.LegadoId, "ux_tipo_produto_legado")
                .IsUnique()
                .HasFilter("(legado_id IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
