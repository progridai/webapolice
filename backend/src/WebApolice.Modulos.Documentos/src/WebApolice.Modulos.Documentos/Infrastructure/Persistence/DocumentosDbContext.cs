using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Documentos.src.WebApolice.Modulos.Documentos.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Documentos.src.WebApolice.Modulos.Documentos.Infrastructure.Persistence;

public partial class DocumentosDbContext : DbContext
{
    public DocumentosDbContext()
    {
    }

    public DocumentosDbContext(DbContextOptions<DocumentosDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Arquivo> Arquivos { get; set; }

    public virtual DbSet<ArquivoAcessoLog> ArquivoAcessoLogs { get; set; }

    public virtual DbSet<ArquivoVersao> ArquivoVersaos { get; set; }

    public virtual DbSet<ArquivoVinculo> ArquivoVinculos { get; set; }

    public virtual DbSet<StorageProvider> StorageProviders { get; set; }

    public virtual DbSet<TipoAnexo> TipoAnexos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=painel.bravida.com.br;Port=5432;Database=webapolice_teste;Username=bravito;Password=Bravida@2023!");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("unaccent");

        modelBuilder.Entity<Arquivo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("arquivo_pkey");

            entity.ToTable("arquivo", "documento");

            entity.HasIndex(e => e.DataDocumento, "ix_arquivo_data_documento");

            entity.HasIndex(e => e.Extensao, "ix_arquivo_extensao");

            entity.HasIndex(e => e.ExtensaoNormalizada, "ix_arquivo_extensao_normalizada");

            entity.HasIndex(e => e.HashSha256, "ix_arquivo_hash");

            entity.HasIndex(e => e.MigracaoStatus, "ix_arquivo_migracao_status");

            entity.HasIndex(e => e.PublicId, "ix_arquivo_public_id");

            entity.HasIndex(e => e.Status, "ix_arquivo_status");

            entity.HasIndex(e => e.StorageKey, "ix_arquivo_storage_key");

            entity.HasIndex(e => e.LegadoId, "ux_arquivo_legado")
                .IsUnique()
                .HasFilter("(legado_id IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArquivoLegado)
                .HasMaxLength(255)
                .HasColumnName("arquivo_legado");
            entity.Property(e => e.Bucket)
                .HasMaxLength(120)
                .HasColumnName("bucket");
            entity.Property(e => e.CaminhoLegado).HasColumnName("caminho_legado");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CriadoPorUsuarioId).HasColumnName("criado_por_usuario_id");
            entity.Property(e => e.CriadoPorUsuarioLegadoId).HasColumnName("criado_por_usuario_legado_id");
            entity.Property(e => e.DataDocumento).HasColumnName("data_documento");
            entity.Property(e => e.DataUpload).HasColumnName("data_upload");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Descricao).HasColumnName("descricao");
            entity.Property(e => e.Extensao)
                .HasMaxLength(20)
                .HasColumnName("extensao");
            entity.Property(e => e.ExtensaoConfiavel)
                .HasDefaultValue(true)
                .HasColumnName("extensao_confiavel");
            entity.Property(e => e.ExtensaoNormalizada)
                .HasMaxLength(20)
                .HasColumnName("extensao_normalizada");
            entity.Property(e => e.ExtensaoOriginal)
                .HasMaxLength(50)
                .HasColumnName("extensao_original");
            entity.Property(e => e.HashSha256)
                .HasMaxLength(64)
                .HasColumnName("hash_sha256");
            entity.Property(e => e.HoraOriginal)
                .HasMaxLength(20)
                .HasColumnName("hora_original");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.MigracaoErro).HasColumnName("migracao_erro");
            entity.Property(e => e.MigracaoStatus)
                .HasMaxLength(40)
                .HasDefaultValueSql("'pendente'::character varying")
                .HasColumnName("migracao_status");
            entity.Property(e => e.MimeType)
                .HasMaxLength(120)
                .HasColumnName("mime_type");
            entity.Property(e => e.NomeArmazenado)
                .HasMaxLength(255)
                .HasColumnName("nome_armazenado");
            entity.Property(e => e.NomeOriginal)
                .HasMaxLength(255)
                .HasColumnName("nome_original");
            entity.Property(e => e.Origem)
                .HasMaxLength(50)
                .HasDefaultValueSql("'legado'::character varying")
                .HasColumnName("origem");
            entity.Property(e => e.PublicId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("public_id");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValueSql("'ativo'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.StorageKey).HasColumnName("storage_key");
            entity.Property(e => e.StoragePath).HasColumnName("storage_path");
            entity.Property(e => e.StorageProviderId).HasColumnName("storage_provider_id");
            entity.Property(e => e.TamanhoBytes).HasColumnName("tamanho_bytes");
            entity.Property(e => e.Titulo)
                .HasMaxLength(300)
                .HasColumnName("titulo");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.StorageProvider).WithMany(p => p.Arquivos)
                .HasForeignKey(d => d.StorageProviderId)
                .HasConstraintName("arquivo_storage_provider_id_fkey");
        });

        modelBuilder.Entity<ArquivoAcessoLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("arquivo_acesso_log_pkey");

            entity.ToTable("arquivo_acesso_log", "documento");

            entity.HasIndex(e => e.ArquivoId, "ix_arquivo_acesso_log_arquivo");

            entity.HasIndex(e => e.CreatedAt, "ix_arquivo_acesso_log_data");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Acao)
                .HasMaxLength(40)
                .HasColumnName("acao");
            entity.Property(e => e.ArquivoId).HasColumnName("arquivo_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IpOrigem)
                .HasMaxLength(80)
                .HasColumnName("ip_origem");
            entity.Property(e => e.UserAgent).HasColumnName("user_agent");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.UsuarioLegadoId).HasColumnName("usuario_legado_id");

            entity.HasOne(d => d.Arquivo).WithMany(p => p.ArquivoAcessoLogs)
                .HasForeignKey(d => d.ArquivoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("arquivo_acesso_log_arquivo_id_fkey");
        });

        modelBuilder.Entity<ArquivoVersao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("arquivo_versao_pkey");

            entity.ToTable("arquivo_versao", "documento");

            entity.HasIndex(e => new { e.ArquivoId, e.Versao }, "arquivo_versao_arquivo_id_versao_key").IsUnique();

            entity.HasIndex(e => e.ArquivoId, "ix_arquivo_versao_arquivo");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArquivoId).HasColumnName("arquivo_id");
            entity.Property(e => e.Bucket)
                .HasMaxLength(120)
                .HasColumnName("bucket");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CriadoPorUsuarioId).HasColumnName("criado_por_usuario_id");
            entity.Property(e => e.CriadoPorUsuarioLegadoId).HasColumnName("criado_por_usuario_legado_id");
            entity.Property(e => e.Extensao)
                .HasMaxLength(20)
                .HasColumnName("extensao");
            entity.Property(e => e.HashSha256)
                .HasMaxLength(64)
                .HasColumnName("hash_sha256");
            entity.Property(e => e.MimeType)
                .HasMaxLength(120)
                .HasColumnName("mime_type");
            entity.Property(e => e.Motivo)
                .HasMaxLength(150)
                .HasColumnName("motivo");
            entity.Property(e => e.NomeOriginal)
                .HasMaxLength(255)
                .HasColumnName("nome_original");
            entity.Property(e => e.StorageKey).HasColumnName("storage_key");
            entity.Property(e => e.StoragePath).HasColumnName("storage_path");
            entity.Property(e => e.StorageProviderId).HasColumnName("storage_provider_id");
            entity.Property(e => e.TamanhoBytes).HasColumnName("tamanho_bytes");
            entity.Property(e => e.Versao)
                .HasDefaultValue(1)
                .HasColumnName("versao");

            entity.HasOne(d => d.Arquivo).WithMany(p => p.ArquivoVersaos)
                .HasForeignKey(d => d.ArquivoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("arquivo_versao_arquivo_id_fkey");

            entity.HasOne(d => d.StorageProvider).WithMany(p => p.ArquivoVersaos)
                .HasForeignKey(d => d.StorageProviderId)
                .HasConstraintName("arquivo_versao_storage_provider_id_fkey");
        });

        modelBuilder.Entity<ArquivoVinculo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("arquivo_vinculo_pkey");

            entity.ToTable("arquivo_vinculo", "documento");

            entity.HasIndex(e => e.ArquivoId, "ix_arquivo_vinculo_arquivo");

            entity.HasIndex(e => new { e.EntidadeTipo, e.EntidadeId }, "ix_arquivo_vinculo_entidade");

            entity.HasIndex(e => new { e.EntidadeTipo, e.EntidadeLegadoId }, "ix_arquivo_vinculo_legado");

            entity.HasIndex(e => new { e.EntidadeLegadoTipo, e.EntidadeLegadoChave1, e.EntidadeLegadoChave2 }, "ix_arquivo_vinculo_legado_chaves");

            entity.HasIndex(e => e.TipoAnexoId, "ix_arquivo_vinculo_tipo_anexo");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArquivoId).HasColumnName("arquivo_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CriterioResolucao)
                .HasMaxLength(100)
                .HasColumnName("criterio_resolucao");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.EntidadeId).HasColumnName("entidade_id");
            entity.Property(e => e.EntidadeLegadoChave1)
                .HasMaxLength(80)
                .HasColumnName("entidade_legado_chave_1");
            entity.Property(e => e.EntidadeLegadoChave2)
                .HasMaxLength(80)
                .HasColumnName("entidade_legado_chave_2");
            entity.Property(e => e.EntidadeLegadoChaveConcatenada)
                .HasMaxLength(120)
                .HasColumnName("entidade_legado_chave_concatenada");
            entity.Property(e => e.EntidadeLegadoId).HasColumnName("entidade_legado_id");
            entity.Property(e => e.EntidadeLegadoTipo)
                .HasMaxLength(50)
                .HasColumnName("entidade_legado_tipo");
            entity.Property(e => e.EntidadeTipo)
                .HasMaxLength(50)
                .HasColumnName("entidade_tipo");
            entity.Property(e => e.LegadoOrigemColuna)
                .HasMaxLength(80)
                .HasColumnName("legado_origem_coluna");
            entity.Property(e => e.Obrigatorio).HasColumnName("obrigatorio");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.Principal).HasColumnName("principal");
            entity.Property(e => e.TipoAnexoId).HasColumnName("tipo_anexo_id");
            entity.Property(e => e.VinculoResolvido)
                .HasDefaultValue(true)
                .HasColumnName("vinculo_resolvido");

            entity.HasOne(d => d.Arquivo).WithMany(p => p.ArquivoVinculos)
                .HasForeignKey(d => d.ArquivoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("arquivo_vinculo_arquivo_id_fkey");

            entity.HasOne(d => d.TipoAnexo).WithMany(p => p.ArquivoVinculos)
                .HasForeignKey(d => d.TipoAnexoId)
                .HasConstraintName("arquivo_vinculo_tipo_anexo_id_fkey");
        });

        modelBuilder.Entity<StorageProvider>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("storage_provider_pkey");

            entity.ToTable("storage_provider", "documento");

            entity.HasIndex(e => e.Codigo, "storage_provider_codigo_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.Codigo)
                .HasMaxLength(50)
                .HasColumnName("codigo");
            entity.Property(e => e.Descricao).HasColumnName("descricao");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");
        });

        modelBuilder.Entity<TipoAnexo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tipo_anexo_pkey");

            entity.ToTable("tipo_anexo", "documento");

            entity.HasIndex(e => e.Nome, "ix_tipo_anexo_nome");

            entity.HasIndex(e => e.Codigo, "ux_tipo_anexo_codigo")
                .IsUnique()
                .HasFilter("(codigo IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.Categoria)
                .HasMaxLength(60)
                .HasColumnName("categoria");
            entity.Property(e => e.Codigo)
                .HasMaxLength(80)
                .HasColumnName("codigo");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Descricao).HasColumnName("descricao");
            entity.Property(e => e.ExigeAssinatura).HasColumnName("exige_assinatura");
            entity.Property(e => e.ExigeValidade).HasColumnName("exige_validade");
            entity.Property(e => e.LegadoValorOriginal)
                .HasMaxLength(120)
                .HasColumnName("legado_valor_original");
            entity.Property(e => e.Nome)
                .HasMaxLength(120)
                .HasColumnName("nome");
            entity.Property(e => e.Sensivel).HasColumnName("sensivel");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
