using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApolice.Shared.Infrastructure.Persistence.Models;

namespace WebApolice.Shared.Infrastructure.Persistence;

public partial class InfraestruturaDbContext : DbContext
{
    public InfraestruturaDbContext()
    {
    }

    public InfraestruturaDbContext(DbContextOptions<InfraestruturaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AgenciadorMigrationMap> AgenciadorMigrationMaps { get; set; }

    public virtual DbSet<AgenciamentoCorretoraLancamentoMigrationMap> AgenciamentoCorretoraLancamentoMigrationMaps { get; set; }

    public virtual DbSet<ClienteMigrationMap> ClienteMigrationMaps { get; set; }

    public virtual DbSet<CoberturaMigrationMap> CoberturaMigrationMaps { get; set; }

    public virtual DbSet<CorretoraMigrationMap> CorretoraMigrationMaps { get; set; }

    public virtual DbSet<DocumentoAnexoMigrationMap> DocumentoAnexoMigrationMaps { get; set; }

    public virtual DbSet<EstipulanteMigrationMap> EstipulanteMigrationMaps { get; set; }

    public virtual DbSet<MovimentoPropostaMigrationMap> MovimentoPropostaMigrationMaps { get; set; }

    public virtual DbSet<PlanoMigrationMap> PlanoMigrationMaps { get; set; }

    public virtual DbSet<ProdutoMigrationMap> ProdutoMigrationMaps { get; set; }

    public virtual DbSet<PropostaBeneficiarioMigrationMap> PropostaBeneficiarioMigrationMaps { get; set; }

    public virtual DbSet<PropostaCoberturaMigrationMap> PropostaCoberturaMigrationMaps { get; set; }

    public virtual DbSet<PropostaItemMigrationMap> PropostaItemMigrationMaps { get; set; }

    public virtual DbSet<PropostaMigrationMap> PropostaMigrationMaps { get; set; }

    public virtual DbSet<PropostaParticipanteMigrationMap> PropostaParticipanteMigrationMaps { get; set; }

    public virtual DbSet<ProtocoloAcompanhamentoMigrationMap> ProtocoloAcompanhamentoMigrationMaps { get; set; }

    public virtual DbSet<ProtocoloItemMigrationMap> ProtocoloItemMigrationMaps { get; set; }

    public virtual DbSet<ProtocoloLoteMigrationMap> ProtocoloLoteMigrationMaps { get; set; }

    public virtual DbSet<SinistroAcompanhamentoMigrationMap> SinistroAcompanhamentoMigrationMaps { get; set; }

    public virtual DbSet<SinistroMigrationMap> SinistroMigrationMaps { get; set; }

    public virtual DbSet<TabelaPrecoMigrationMap> TabelaPrecoMigrationMaps { get; set; }

    public virtual DbSet<TipoProdutoMigrationMap> TipoProdutoMigrationMaps { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=painel.bravida.com.br;Port=5432;Database=webapolice_teste;Username=bravito;Password=Bravida@2023!");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("unaccent");

        modelBuilder.Entity<AgenciadorMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("agenciador_migration_map_pkey");

            entity.ToTable("agenciador_migration_map", "legado");

            entity.HasIndex(e => e.LegadoAgenciadorId, "agenciador_migration_map_legado_agenciador_id_key").IsUnique();

            entity.HasIndex(e => e.AgenciadorId, "ix_agenciador_map_agenciador");

            entity.HasIndex(e => e.CpfLimpo, "ix_agenciador_map_cpf");

            entity.HasIndex(e => e.PessoaId, "ix_agenciador_map_pessoa");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AgenciadorId).HasColumnName("agenciador_id");
            entity.Property(e => e.CoordenadorId).HasColumnName("coordenador_id");
            entity.Property(e => e.CpfLimpo)
                .HasMaxLength(20)
                .HasColumnName("cpf_limpo");
            entity.Property(e => e.CpfOriginal)
                .HasMaxLength(30)
                .HasColumnName("cpf_original");
            entity.Property(e => e.CpfValido).HasColumnName("cpf_valido");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CriterioMigracao)
                .HasMaxLength(100)
                .HasColumnName("criterio_migracao");
            entity.Property(e => e.LegadoAgenciadorId).HasColumnName("legado_agenciador_id");
            entity.Property(e => e.LegadoCoordenadorId).HasColumnName("legado_coordenador_id");
            entity.Property(e => e.NomeOriginal)
                .HasMaxLength(150)
                .HasColumnName("nome_original");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
        });

        modelBuilder.Entity<AgenciamentoCorretoraLancamentoMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("agenciamento_corretora_lancamento_migration_map_pkey");

            entity.ToTable("agenciamento_corretora_lancamento_migration_map", "legado");

            entity.HasIndex(e => e.LegadoAgenciamentoId, "agenciamento_corretora_lancamento_mi_legado_agenciamento_id_key").IsUnique();

            entity.HasIndex(e => e.CorretoraId, "ix_agenciamento_corretora_map_corretora");

            entity.HasIndex(e => e.PropostaId, "ix_agenciamento_corretora_map_proposta");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AgenciamentoCorretoraLancamentoId).HasColumnName("agenciamento_corretora_lancamento_id");
            entity.Property(e => e.CorretoraId).HasColumnName("corretora_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CriterioMigracao)
                .HasMaxLength(100)
                .HasColumnName("criterio_migracao");
            entity.Property(e => e.LegadoAgenciamentoId).HasColumnName("legado_agenciamento_id");
            entity.Property(e => e.LegadoCorretoraId).HasColumnName("legado_corretora_id");
            entity.Property(e => e.LegadoMovimentoId).HasColumnName("legado_movimento_id");
            entity.Property(e => e.LegadoPropostaId).HasColumnName("legado_proposta_id");
            entity.Property(e => e.MovimentoTipoId).HasColumnName("movimento_tipo_id");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.PropostaId).HasColumnName("proposta_id");
        });

        modelBuilder.Entity<ClienteMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cliente_migration_map_pkey");

            entity.ToTable("cliente_migration_map", "legado");

            entity.HasIndex(e => e.LegadoClienteId, "cliente_migration_map_legado_cliente_id_key").IsUnique();

            entity.HasIndex(e => e.ClienteId, "ix_cliente_migration_map_cliente");

            entity.HasIndex(e => e.CpfLimpo, "ix_cliente_migration_map_cpf");

            entity.HasIndex(e => e.PessoaId, "ix_cliente_migration_map_pessoa");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.ClienteVinculoId).HasColumnName("cliente_vinculo_id");
            entity.Property(e => e.CpfLimpo)
                .HasMaxLength(20)
                .HasColumnName("cpf_limpo");
            entity.Property(e => e.CpfOriginal)
                .HasMaxLength(30)
                .HasColumnName("cpf_original");
            entity.Property(e => e.CpfValido).HasColumnName("cpf_valido");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CriterioCriacaoVinculo)
                .HasMaxLength(80)
                .HasColumnName("criterio_criacao_vinculo");
            entity.Property(e => e.CriterioUnificacaoPessoa)
                .HasMaxLength(80)
                .HasColumnName("criterio_unificacao_pessoa");
            entity.Property(e => e.LegadoClienteId).HasColumnName("legado_cliente_id");
            entity.Property(e => e.MatriculaOriginal)
                .HasMaxLength(50)
                .HasColumnName("matricula_original");
            entity.Property(e => e.NomeOriginal)
                .HasMaxLength(150)
                .HasColumnName("nome_original");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
        });

        modelBuilder.Entity<CoberturaMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cobertura_migration_map_pkey");

            entity.ToTable("cobertura_migration_map", "legado");

            entity.HasIndex(e => e.LegadoCoberturaId, "cobertura_migration_map_legado_cobertura_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CoberturaId).HasColumnName("cobertura_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.LegadoCoberturaId).HasColumnName("legado_cobertura_id");
            entity.Property(e => e.NomeOriginal)
                .HasMaxLength(150)
                .HasColumnName("nome_original");
        });

        modelBuilder.Entity<CorretoraMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("corretora_migration_map_pkey");

            entity.ToTable("corretora_migration_map", "legado");

            entity.HasIndex(e => e.LegadoCorretoraId, "corretora_migration_map_legado_corretora_id_key").IsUnique();

            entity.HasIndex(e => e.CorretoraId, "ix_corretora_map_corretora");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CorretoraId).HasColumnName("corretora_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CriterioMigracao)
                .HasMaxLength(100)
                .HasColumnName("criterio_migracao");
            entity.Property(e => e.LegadoCorretoraId).HasColumnName("legado_corretora_id");
            entity.Property(e => e.NomeOriginal)
                .HasMaxLength(150)
                .HasColumnName("nome_original");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
        });

        modelBuilder.Entity<DocumentoAnexoMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("documento_anexo_migration_map_pkey");

            entity.ToTable("documento_anexo_migration_map", "legado");

            entity.HasIndex(e => e.LegadoDocumentoId, "documento_anexo_migration_map_legado_documento_id_key").IsUnique();

            entity.HasIndex(e => e.ArquivoId, "ix_documento_anexo_map_arquivo");

            entity.HasIndex(e => e.ClienteId, "ix_documento_anexo_map_cliente");

            entity.HasIndex(e => e.EstipulanteId, "ix_documento_anexo_map_estipulante");

            entity.HasIndex(e => e.PropostaId, "ix_documento_anexo_map_proposta");

            entity.HasIndex(e => e.SinistroId, "ix_documento_anexo_map_sinistro");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArquivoId).HasColumnName("arquivo_id");
            entity.Property(e => e.ArquivoOriginal)
                .HasMaxLength(255)
                .HasColumnName("arquivo_original");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CriterioMigracao)
                .HasMaxLength(100)
                .HasColumnName("criterio_migracao");
            entity.Property(e => e.EstipulanteId).HasColumnName("estipulante_id");
            entity.Property(e => e.ExtensaoOriginal)
                .HasMaxLength(20)
                .HasColumnName("extensao_original");
            entity.Property(e => e.LegadoDocumentoId).HasColumnName("legado_documento_id");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.PkCliente).HasColumnName("pk_cliente");
            entity.Property(e => e.PkEstipulante).HasColumnName("pk_estipulante");
            entity.Property(e => e.PkProposta).HasColumnName("pk_proposta");
            entity.Property(e => e.PkProtocolo).HasColumnName("pk_protocolo");
            entity.Property(e => e.PkSinistro).HasColumnName("pk_sinistro");
            entity.Property(e => e.PropostaId).HasColumnName("proposta_id");
            entity.Property(e => e.SinistroId).HasColumnName("sinistro_id");
            entity.Property(e => e.TipoAnexoOriginal)
                .HasMaxLength(120)
                .HasColumnName("tipo_anexo_original");
            entity.Property(e => e.TituloOriginal)
                .HasMaxLength(300)
                .HasColumnName("titulo_original");
        });

        modelBuilder.Entity<EstipulanteMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("estipulante_migration_map_pkey");

            entity.ToTable("estipulante_migration_map", "legado");

            entity.HasIndex(e => e.LegadoEstipulanteId, "estipulante_migration_map_legado_estipulante_id_key").IsUnique();

            entity.HasIndex(e => e.CnpjLimpo, "ix_estipulante_migration_map_cnpj");

            entity.HasIndex(e => e.PessoaId, "ix_estipulante_migration_map_pessoa");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CnpjLimpo)
                .HasMaxLength(20)
                .HasColumnName("cnpj_limpo");
            entity.Property(e => e.CnpjOriginal)
                .HasMaxLength(30)
                .HasColumnName("cnpj_original");
            entity.Property(e => e.CnpjValido).HasColumnName("cnpj_valido");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CriterioUnificacaoPessoa)
                .HasMaxLength(80)
                .HasColumnName("criterio_unificacao_pessoa");
            entity.Property(e => e.EstipulanteId).HasColumnName("estipulante_id");
            entity.Property(e => e.LegadoEstipulanteId).HasColumnName("legado_estipulante_id");
            entity.Property(e => e.NomeOriginal)
                .HasMaxLength(150)
                .HasColumnName("nome_original");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
        });

        modelBuilder.Entity<MovimentoPropostaMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("movimento_proposta_migration_map_pkey");

            entity.ToTable("movimento_proposta_migration_map", "legado");

            entity.HasIndex(e => e.ClienteId, "ix_movimento_proposta_map_cliente");

            entity.HasIndex(e => e.EstipulanteId, "ix_movimento_proposta_map_estipulante");

            entity.HasIndex(e => e.PropostaMovimentoId, "ix_movimento_proposta_map_movimento");

            entity.HasIndex(e => e.PropostaId, "ix_movimento_proposta_map_proposta");

            entity.HasIndex(e => e.TituloId, "ix_movimento_proposta_map_titulo");

            entity.HasIndex(e => e.LegadoMovimentoPropostaId, "movimento_proposta_migration_m_legado_movimento_proposta_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Classificacao)
                .HasMaxLength(40)
                .HasColumnName("classificacao");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.ClienteVinculoId).HasColumnName("cliente_vinculo_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CriterioMigracao)
                .HasMaxLength(100)
                .HasColumnName("criterio_migracao");
            entity.Property(e => e.EstipulanteId).HasColumnName("estipulante_id");
            entity.Property(e => e.LancamentoComissaoId).HasColumnName("lancamento_comissao_id");
            entity.Property(e => e.LegadoClienteId).HasColumnName("legado_cliente_id");
            entity.Property(e => e.LegadoEstipulanteId).HasColumnName("legado_estipulante_id");
            entity.Property(e => e.LegadoMovimentoId).HasColumnName("legado_movimento_id");
            entity.Property(e => e.LegadoMovimentoPropostaId).HasColumnName("legado_movimento_proposta_id");
            entity.Property(e => e.LegadoPropostaId).HasColumnName("legado_proposta_id");
            entity.Property(e => e.MovimentoTipoId).HasColumnName("movimento_tipo_id");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.PropostaId).HasColumnName("proposta_id");
            entity.Property(e => e.PropostaMovimentoId).HasColumnName("proposta_movimento_id");
            entity.Property(e => e.TituloId).HasColumnName("titulo_id");
            entity.Property(e => e.TituloPagamentoId).HasColumnName("titulo_pagamento_id");
            entity.Property(e => e.TituloRetornoBancarioId).HasColumnName("titulo_retorno_bancario_id");
        });

        modelBuilder.Entity<PlanoMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("plano_migration_map_pkey");

            entity.ToTable("plano_migration_map", "legado");

            entity.HasIndex(e => e.LegadoPlanoId, "plano_migration_map_legado_plano_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.LegadoPlanoId).HasColumnName("legado_plano_id");
            entity.Property(e => e.NomeOriginal)
                .HasMaxLength(150)
                .HasColumnName("nome_original");
            entity.Property(e => e.PlanoId).HasColumnName("plano_id");
        });

        modelBuilder.Entity<ProdutoMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("produto_migration_map_pkey");

            entity.ToTable("produto_migration_map", "legado");

            entity.HasIndex(e => e.LegadoProdutoId, "produto_migration_map_legado_produto_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CodigoReferenciaOriginal)
                .HasMaxLength(80)
                .HasColumnName("codigo_referencia_original");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.LegadoProdutoId).HasColumnName("legado_produto_id");
            entity.Property(e => e.ProdutoId).HasColumnName("produto_id");
        });

        modelBuilder.Entity<PropostaBeneficiarioMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("proposta_beneficiario_migration_map_pkey");

            entity.ToTable("proposta_beneficiario_migration_map", "legado");

            entity.HasIndex(e => e.CpfLimpo, "ix_proposta_beneficiario_map_cpf");

            entity.HasIndex(e => e.PessoaId, "ix_proposta_beneficiario_map_pessoa");

            entity.HasIndex(e => e.PropostaId, "ix_proposta_beneficiario_map_proposta");

            entity.HasIndex(e => e.LegadoBeneficiarioId, "proposta_beneficiario_migration_map_legado_beneficiario_id_key").IsUnique();

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
            entity.Property(e => e.CriterioMigracao)
                .HasMaxLength(100)
                .HasColumnName("criterio_migracao");
            entity.Property(e => e.LegadoBeneficiarioId).HasColumnName("legado_beneficiario_id");
            entity.Property(e => e.LegadoPropostaId).HasColumnName("legado_proposta_id");
            entity.Property(e => e.NomeOriginal)
                .HasMaxLength(150)
                .HasColumnName("nome_original");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.ParentescoNormalizado)
                .HasMaxLength(60)
                .HasColumnName("parentesco_normalizado");
            entity.Property(e => e.ParentescoOriginal)
                .HasMaxLength(100)
                .HasColumnName("parentesco_original");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.PropostaBeneficiarioId).HasColumnName("proposta_beneficiario_id");
            entity.Property(e => e.PropostaId).HasColumnName("proposta_id");
        });

        modelBuilder.Entity<PropostaCoberturaMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("proposta_cobertura_migration_map_pkey");

            entity.ToTable("proposta_cobertura_migration_map", "legado");

            entity.HasIndex(e => e.PropostaItemId, "ix_proposta_cobertura_migration_map_item");

            entity.HasIndex(e => e.PropostaId, "ix_proposta_cobertura_migration_map_proposta");

            entity.HasIndex(e => e.LegadoPropostaCoberturaId, "proposta_cobertura_migration_m_legado_proposta_cobertura_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CoberturaId).HasColumnName("cobertura_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CriterioMigracao)
                .HasMaxLength(100)
                .HasColumnName("criterio_migracao");
            entity.Property(e => e.LegadoCoberturaId).HasColumnName("legado_cobertura_id");
            entity.Property(e => e.LegadoPropostaCoberturaId).HasColumnName("legado_proposta_cobertura_id");
            entity.Property(e => e.LegadoPropostaId).HasColumnName("legado_proposta_id");
            entity.Property(e => e.LegadoPropostaTipoId).HasColumnName("legado_proposta_tipo_id");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.PropostaCoberturaId).HasColumnName("proposta_cobertura_id");
            entity.Property(e => e.PropostaId).HasColumnName("proposta_id");
            entity.Property(e => e.PropostaItemId).HasColumnName("proposta_item_id");
        });

        modelBuilder.Entity<PropostaItemMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("proposta_item_migration_map_pkey");

            entity.ToTable("proposta_item_migration_map", "legado");

            entity.HasIndex(e => e.PropostaItemId, "ix_proposta_item_migration_map_item");

            entity.HasIndex(e => e.PropostaId, "ix_proposta_item_migration_map_proposta");

            entity.HasIndex(e => e.LegadoPropostaTipoId, "proposta_item_migration_map_legado_proposta_tipo_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CriterioMigracao)
                .HasMaxLength(100)
                .HasColumnName("criterio_migracao");
            entity.Property(e => e.LegadoPlanoOriginal)
                .HasMaxLength(100)
                .HasColumnName("legado_plano_original");
            entity.Property(e => e.LegadoProdutoId).HasColumnName("legado_produto_id");
            entity.Property(e => e.LegadoPropostaId).HasColumnName("legado_proposta_id");
            entity.Property(e => e.LegadoPropostaTipoId).HasColumnName("legado_proposta_tipo_id");
            entity.Property(e => e.LegadoTabelaId).HasColumnName("legado_tabela_id");
            entity.Property(e => e.LegadoTipoId).HasColumnName("legado_tipo_id");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.PlanoId).HasColumnName("plano_id");
            entity.Property(e => e.ProdutoId).HasColumnName("produto_id");
            entity.Property(e => e.PropostaId).HasColumnName("proposta_id");
            entity.Property(e => e.PropostaItemId).HasColumnName("proposta_item_id");
            entity.Property(e => e.TabelaPrecoId).HasColumnName("tabela_preco_id");
            entity.Property(e => e.TipoProdutoId).HasColumnName("tipo_produto_id");
        });

        modelBuilder.Entity<PropostaMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("proposta_migration_map_pkey");

            entity.ToTable("proposta_migration_map", "legado");

            entity.HasIndex(e => e.ClienteId, "ix_proposta_migration_map_cliente");

            entity.HasIndex(e => e.EstipulanteId, "ix_proposta_migration_map_estipulante");

            entity.HasIndex(e => e.PropostaId, "ix_proposta_migration_map_proposta");

            entity.HasIndex(e => e.ClienteVinculoId, "ix_proposta_migration_map_vinculo");

            entity.HasIndex(e => e.LegadoPropostaId, "proposta_migration_map_legado_proposta_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.ClienteVinculoId).HasColumnName("cliente_vinculo_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CriterioMigracao)
                .HasMaxLength(100)
                .HasColumnName("criterio_migracao");
            entity.Property(e => e.EstipulanteId).HasColumnName("estipulante_id");
            entity.Property(e => e.LegadoClienteId).HasColumnName("legado_cliente_id");
            entity.Property(e => e.LegadoEstipulanteId).HasColumnName("legado_estipulante_id");
            entity.Property(e => e.LegadoPropostaId).HasColumnName("legado_proposta_id");
            entity.Property(e => e.LegadoStatus).HasColumnName("legado_status");
            entity.Property(e => e.LegadoSubestipulanteId).HasColumnName("legado_subestipulante_id");
            entity.Property(e => e.NumeroOriginal)
                .HasMaxLength(100)
                .HasColumnName("numero_original");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.PropostaId).HasColumnName("proposta_id");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.SubestipulanteId).HasColumnName("subestipulante_id");
        });

        modelBuilder.Entity<PropostaParticipanteMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("proposta_participante_migration_map_pkey");

            entity.ToTable("proposta_participante_migration_map", "legado");

            entity.HasIndex(e => e.AgenciadorId, "ix_proposta_participante_map_agenciador");

            entity.HasIndex(e => e.CorretoraId, "ix_proposta_participante_map_corretora");

            entity.HasIndex(e => e.PropostaId, "ix_proposta_participante_map_proposta");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AgenciadorId).HasColumnName("agenciador_id");
            entity.Property(e => e.CampoOrigem)
                .HasMaxLength(80)
                .HasColumnName("campo_origem");
            entity.Property(e => e.CodigoLegadoParticipante).HasColumnName("codigo_legado_participante");
            entity.Property(e => e.CorretoraId).HasColumnName("corretora_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CriterioMigracao)
                .HasMaxLength(100)
                .HasColumnName("criterio_migracao");
            entity.Property(e => e.LegadoPropostaId).HasColumnName("legado_proposta_id");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.ParticipanteTipo)
                .HasMaxLength(40)
                .HasColumnName("participante_tipo");
            entity.Property(e => e.PropostaId).HasColumnName("proposta_id");
            entity.Property(e => e.PropostaParticipanteId).HasColumnName("proposta_participante_id");
        });

        modelBuilder.Entity<ProtocoloAcompanhamentoMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("protocolo_acompanhamento_migration_map_pkey");

            entity.ToTable("protocolo_acompanhamento_migration_map", "legado");

            entity.HasIndex(e => e.ProtocoloLoteId, "ix_protocolo_acompanhamento_map_lote");

            entity.HasIndex(e => e.LegadoAcompanhamentoId, "protocolo_acompanhamento_migration_legado_acompanhamento_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CriterioMigracao)
                .HasMaxLength(100)
                .HasColumnName("criterio_migracao");
            entity.Property(e => e.LegadoAcompanhamentoId).HasColumnName("legado_acompanhamento_id");
            entity.Property(e => e.LegadoProtocoloId).HasColumnName("legado_protocolo_id");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.ProtocoloAcompanhamentoId).HasColumnName("protocolo_acompanhamento_id");
            entity.Property(e => e.ProtocoloLoteId).HasColumnName("protocolo_lote_id");
        });

        modelBuilder.Entity<ProtocoloItemMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("protocolo_item_migration_map_pkey");

            entity.ToTable("protocolo_item_migration_map", "legado");

            entity.HasIndex(e => e.ClienteId, "ix_protocolo_item_map_cliente");

            entity.HasIndex(e => e.ProtocoloItemId, "ix_protocolo_item_map_item");

            entity.HasIndex(e => e.ProtocoloLoteId, "ix_protocolo_item_map_lote");

            entity.HasIndex(e => new { e.OrigemLegado, e.LegadoClienteProtocoloId }, "protocolo_item_migration_map_origem_legado_legado_cliente_p_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.ClienteVinculoId).HasColumnName("cliente_vinculo_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CriterioMigracao)
                .HasMaxLength(100)
                .HasColumnName("criterio_migracao");
            entity.Property(e => e.EstipulanteId).HasColumnName("estipulante_id");
            entity.Property(e => e.LegadoClienteId).HasColumnName("legado_cliente_id");
            entity.Property(e => e.LegadoClienteProtocoloId).HasColumnName("legado_cliente_protocolo_id");
            entity.Property(e => e.LegadoEstipulanteId).HasColumnName("legado_estipulante_id");
            entity.Property(e => e.LegadoProtocoloId).HasColumnName("legado_protocolo_id");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.OrigemLegado)
                .HasMaxLength(80)
                .HasColumnName("origem_legado");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.ProtocoloItemId).HasColumnName("protocolo_item_id");
            entity.Property(e => e.ProtocoloLoteId).HasColumnName("protocolo_lote_id");
        });

        modelBuilder.Entity<ProtocoloLoteMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("protocolo_lote_migration_map_pkey");

            entity.ToTable("protocolo_lote_migration_map", "legado");

            entity.HasIndex(e => e.ProtocoloLoteId, "ix_protocolo_lote_map_lote");

            entity.HasIndex(e => e.LegadoProtocoloId, "protocolo_lote_migration_map_legado_protocolo_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CriterioMigracao)
                .HasMaxLength(100)
                .HasColumnName("criterio_migracao");
            entity.Property(e => e.DataProtocoloOriginal).HasColumnName("data_protocolo_original");
            entity.Property(e => e.LegadoProtocoloId).HasColumnName("legado_protocolo_id");
            entity.Property(e => e.NumeroProtocoloOriginal).HasColumnName("numero_protocolo_original");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.ProtocoloLoteId).HasColumnName("protocolo_lote_id");
        });

        modelBuilder.Entity<SinistroAcompanhamentoMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sinistro_acompanhamento_migration_map_pkey");

            entity.ToTable("sinistro_acompanhamento_migration_map", "legado");

            entity.HasIndex(e => e.SinistroId, "ix_sinistro_acompanhamento_map_sinistro");

            entity.HasIndex(e => e.LegadoAcompanhamentoId, "sinistro_acompanhamento_migration__legado_acompanhamento_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AcompanhamentoId).HasColumnName("acompanhamento_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CriterioMigracao)
                .HasMaxLength(100)
                .HasColumnName("criterio_migracao");
            entity.Property(e => e.LegadoAcompanhamentoId).HasColumnName("legado_acompanhamento_id");
            entity.Property(e => e.LegadoSinistroId).HasColumnName("legado_sinistro_id");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.SinistroId).HasColumnName("sinistro_id");
        });

        modelBuilder.Entity<SinistroMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sinistro_migration_map_pkey");

            entity.ToTable("sinistro_migration_map", "legado");

            entity.HasIndex(e => e.ClienteId, "ix_sinistro_migration_map_cliente");

            entity.HasIndex(e => e.PropostaId, "ix_sinistro_migration_map_proposta");

            entity.HasIndex(e => e.SinistroId, "ix_sinistro_migration_map_sinistro");

            entity.HasIndex(e => e.LegadoSinistroId, "sinistro_migration_map_legado_sinistro_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.ClienteVinculoId).HasColumnName("cliente_vinculo_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CriterioMigracao)
                .HasMaxLength(100)
                .HasColumnName("criterio_migracao");
            entity.Property(e => e.LegadoPropostaId).HasColumnName("legado_proposta_id");
            entity.Property(e => e.LegadoSinistroId).HasColumnName("legado_sinistro_id");
            entity.Property(e => e.LegadoStatus).HasColumnName("legado_status");
            entity.Property(e => e.NumeroSinistroOriginal)
                .HasMaxLength(80)
                .HasColumnName("numero_sinistro_original");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.PropostaId).HasColumnName("proposta_id");
            entity.Property(e => e.SinistroId).HasColumnName("sinistro_id");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
        });

        modelBuilder.Entity<TabelaPrecoMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tabela_preco_migration_map_pkey");

            entity.ToTable("tabela_preco_migration_map", "legado");

            entity.HasIndex(e => e.LegadoTabelaId, "tabela_preco_migration_map_legado_tabela_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.LegadoTabelaId).HasColumnName("legado_tabela_id");
            entity.Property(e => e.NomeOriginal)
                .HasMaxLength(150)
                .HasColumnName("nome_original");
            entity.Property(e => e.TabelaPrecoId).HasColumnName("tabela_preco_id");
        });

        modelBuilder.Entity<TipoProdutoMigrationMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tipo_produto_migration_map_pkey");

            entity.ToTable("tipo_produto_migration_map", "legado");

            entity.HasIndex(e => e.LegadoTipoId, "tipo_produto_migration_map_legado_tipo_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.LegadoTipoId).HasColumnName("legado_tipo_id");
            entity.Property(e => e.NomeOriginal)
                .HasMaxLength(100)
                .HasColumnName("nome_original");
            entity.Property(e => e.TipoProdutoId).HasColumnName("tipo_produto_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
