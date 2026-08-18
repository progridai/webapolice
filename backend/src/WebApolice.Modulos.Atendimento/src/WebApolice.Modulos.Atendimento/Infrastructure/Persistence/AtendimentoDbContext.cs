using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Atendimento.src.WebApolice.Modulos.Atendimento.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Atendimento.src.WebApolice.Modulos.Atendimento.Infrastructure.Persistence;

public partial class AtendimentoDbContext : DbContext
{
    public AtendimentoDbContext()
    {
    }

    public AtendimentoDbContext(DbContextOptions<AtendimentoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ProtocoloAcompanhamento> ProtocoloAcompanhamentos { get; set; }

    public virtual DbSet<ProtocoloItem> ProtocoloItems { get; set; }

    public virtual DbSet<ProtocoloLote> ProtocoloLotes { get; set; }

    public virtual DbSet<ProtocoloRelatorioSeguradora> ProtocoloRelatorioSeguradoras { get; set; }

    public virtual DbSet<ProtocoloRelatorioSeguradoraItem> ProtocoloRelatorioSeguradoraItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=painel.bravida.com.br;Port=5432;Database=webapolice_teste;Username=bravito;Password=Bravida@2023!");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("unaccent");

        modelBuilder.Entity<ProtocoloAcompanhamento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("protocolo_acompanhamento_pkey");

            entity.ToTable("protocolo_acompanhamento", "atendimento");

            entity.HasIndex(e => e.DataAcompanhamento, "ix_protocolo_acompanhamento_data");

            entity.HasIndex(e => e.ProtocoloLoteId, "ix_protocolo_acompanhamento_lote");

            entity.HasIndex(e => e.LegadoId, "ux_protocolo_acompanhamento_legado").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
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
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.ProtocoloLoteId).HasColumnName("protocolo_lote_id");
            entity.Property(e => e.UsuarioLegadoId).HasColumnName("usuario_legado_id");

            entity.HasOne(d => d.ProtocoloLote).WithMany(p => p.ProtocoloAcompanhamentos)
                .HasForeignKey(d => d.ProtocoloLoteId)
                .HasConstraintName("protocolo_acompanhamento_protocolo_lote_id_fkey");
        });

        modelBuilder.Entity<ProtocoloItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("protocolo_item_pkey");

            entity.ToTable("protocolo_item", "atendimento");

            entity.HasIndex(e => e.ClienteId, "ix_protocolo_item_cliente");

            entity.HasIndex(e => e.EstipulanteId, "ix_protocolo_item_estipulante");

            entity.HasIndex(e => e.ProtocoloLoteId, "ix_protocolo_item_lote");

            entity.HasIndex(e => e.Matricula, "ix_protocolo_item_matricula");

            entity.HasIndex(e => e.TipoItem, "ix_protocolo_item_tipo");

            entity.HasIndex(e => e.ClienteVinculoId, "ix_protocolo_item_vinculo");

            entity.HasIndex(e => new { e.OrigemLegado, e.LegadoId }, "ux_protocolo_item_legado_origem").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.ClienteVinculoId).HasColumnName("cliente_vinculo_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DataVigencia).HasColumnName("data_vigencia");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Equipe)
                .HasMaxLength(100)
                .HasColumnName("equipe");
            entity.Property(e => e.EstipulanteId).HasColumnName("estipulante_id");
            entity.Property(e => e.LegadoClienteId).HasColumnName("legado_cliente_id");
            entity.Property(e => e.LegadoEstipulanteId).HasColumnName("legado_estipulante_id");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.Matricula)
                .HasMaxLength(80)
                .HasColumnName("matricula");
            entity.Property(e => e.NomeConjuge)
                .HasMaxLength(150)
                .HasColumnName("nome_conjuge");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.OrigemLegado)
                .HasMaxLength(80)
                .HasColumnName("origem_legado");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.Premio)
                .HasPrecision(18, 2)
                .HasColumnName("premio");
            entity.Property(e => e.ProtocoloLoteId).HasColumnName("protocolo_lote_id");
            entity.Property(e => e.TipoItem)
                .HasMaxLength(40)
                .HasDefaultValueSql("'titular'::character varying")
                .HasColumnName("tipo_item");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ProtocoloLote).WithMany(p => p.ProtocoloItems)
                .HasForeignKey(d => d.ProtocoloLoteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("protocolo_item_protocolo_lote_id_fkey");
        });

        modelBuilder.Entity<ProtocoloLote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("protocolo_lote_pkey");

            entity.ToTable("protocolo_lote", "atendimento");

            entity.HasIndex(e => e.ConsultorLegadoId, "ix_protocolo_lote_consultor");

            entity.HasIndex(e => e.DataProtocolo, "ix_protocolo_lote_data");

            entity.HasIndex(e => e.NumeroProtocolo, "ix_protocolo_lote_numero");

            entity.HasIndex(e => e.LegadoId, "ux_protocolo_lote_legado").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AnexoConsultor).HasColumnName("anexo_consultor");
            entity.Property(e => e.AnexoSeguradora).HasColumnName("anexo_seguradora");
            entity.Property(e => e.ConsultorLegadoId).HasColumnName("consultor_legado_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DataProtocolo).HasColumnName("data_protocolo");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.NumeroProtocolo).HasColumnName("numero_protocolo");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.PublicId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("public_id");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValueSql("'ativo'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UsuarioLegadoId).HasColumnName("usuario_legado_id");
        });

        modelBuilder.Entity<ProtocoloRelatorioSeguradora>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("protocolo_relatorio_seguradora_pkey");

            entity.ToTable("protocolo_relatorio_seguradora", "atendimento");

            entity.HasIndex(e => e.DataRelatorio, "ix_protocolo_rel_seg_data");

            entity.HasIndex(e => e.LegadoId, "ux_protocolo_rel_seg_legado").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DataRelatorio).HasColumnName("data_relatorio");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
        });

        modelBuilder.Entity<ProtocoloRelatorioSeguradoraItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("protocolo_relatorio_seguradora_item_pkey");

            entity.ToTable("protocolo_relatorio_seguradora_item", "atendimento");

            entity.HasIndex(e => e.ClienteId, "ix_protocolo_rel_seg_item_cliente");

            entity.HasIndex(e => e.ProtocoloLoteId, "ix_protocolo_rel_seg_item_protocolo");

            entity.HasIndex(e => e.RelatorioId, "ix_protocolo_rel_seg_item_relatorio");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.ClienteVinculoId).HasColumnName("cliente_vinculo_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.LegadoClienteId).HasColumnName("legado_cliente_id");
            entity.Property(e => e.LegadoProtocoloId).HasColumnName("legado_protocolo_id");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.ProtocoloLoteId).HasColumnName("protocolo_lote_id");
            entity.Property(e => e.RelatorioId).HasColumnName("relatorio_id");

            entity.HasOne(d => d.ProtocoloLote).WithMany(p => p.ProtocoloRelatorioSeguradoraItems)
                .HasForeignKey(d => d.ProtocoloLoteId)
                .HasConstraintName("protocolo_relatorio_seguradora_item_protocolo_lote_id_fkey");

            entity.HasOne(d => d.Relatorio).WithMany(p => p.ProtocoloRelatorioSeguradoraItems)
                .HasForeignKey(d => d.RelatorioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("protocolo_relatorio_seguradora_item_relatorio_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
