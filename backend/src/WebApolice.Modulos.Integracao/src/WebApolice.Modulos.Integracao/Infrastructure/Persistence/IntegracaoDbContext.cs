using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Integracao.src.WebApolice.Modulos.Integracao.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Integracao.src.WebApolice.Modulos.Integracao.Infrastructure.Persistence;

public partial class IntegracaoDbContext : DbContext
{
    public IntegracaoDbContext()
    {
    }

    public IntegracaoDbContext(DbContextOptions<IntegracaoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ReferenciaExterna> ReferenciaExternas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=painel.bravida.com.br;Port=5432;Database=webapolice_teste;Username=bravito;Password=Bravida@2023!");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("unaccent");

        modelBuilder.Entity<ReferenciaExterna>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("referencia_externa_pkey");

            entity.ToTable("referencia_externa", "integracao");

            entity.HasIndex(e => new { e.EntidadeTipo, e.EntidadeId }, "ix_referencia_externa_entidade");

            entity.HasIndex(e => new { e.Sistema, e.EntidadeTipo, e.ChaveExterna }, "referencia_externa_sistema_entidade_tipo_chave_externa_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.ChaveExterna)
                .HasMaxLength(150)
                .HasColumnName("chave_externa");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Dados)
                .HasColumnType("jsonb")
                .HasColumnName("dados");
            entity.Property(e => e.EntidadeId).HasColumnName("entidade_id");
            entity.Property(e => e.EntidadeTipo)
                .HasMaxLength(50)
                .HasColumnName("entidade_tipo");
            entity.Property(e => e.Sistema)
                .HasMaxLength(50)
                .HasColumnName("sistema");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
