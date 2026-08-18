using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Sinistros.src.WebApolice.Modulos.Sinistros.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Sinistros.src.WebApolice.Modulos.Sinistros.Infrastructure.Persistence;

public partial class SinistrosDbContext : DbContext
{
    public SinistrosDbContext()
    {
    }

    public SinistrosDbContext(DbContextOptions<SinistrosDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Acompanhamento> Acompanhamentos { get; set; }

    public virtual DbSet<Sinistro> Sinistros { get; set; }

    public virtual DbSet<SinistroBeneficiario> SinistroBeneficiarios { get; set; }

    public virtual DbSet<SinistroCobertura> SinistroCoberturas { get; set; }

    public virtual DbSet<SinistroStatus> SinistroStatuses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=painel.bravida.com.br;Port=5432;Database=webapolice_teste;Username=bravito;Password=Bravida@2023!");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("unaccent");

        modelBuilder.Entity<Acompanhamento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acompanhamento_pkey");

            entity.ToTable("acompanhamento", "sinistro");

            entity.HasIndex(e => e.DataAcompanhamento, "ix_sinistro_acompanhamento_data");

            entity.HasIndex(e => e.SinistroId, "ix_sinistro_acompanhamento_sinistro");

            entity.HasIndex(e => e.LegadoId, "ux_sinistro_acompanhamento_legado").IsUnique();

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
            entity.Property(e => e.SinistroId).HasColumnName("sinistro_id");
            entity.Property(e => e.UsuarioLegadoId).HasColumnName("usuario_legado_id");

            entity.HasOne(d => d.Sinistro).WithMany(p => p.Acompanhamentos)
                .HasForeignKey(d => d.SinistroId)
                .HasConstraintName("acompanhamento_sinistro_id_fkey");
        });

        modelBuilder.Entity<Sinistro>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sinistro_pkey");

            entity.ToTable("sinistro", "sinistro");

            entity.HasIndex(e => e.ClienteId, "ix_sinistro_cliente");

            entity.HasIndex(e => e.CpfSinistradoLimpo, "ix_sinistro_cpf_sinistrado");

            entity.HasIndex(e => e.DataAviso, "ix_sinistro_data_aviso");

            entity.HasIndex(e => e.DataOcorrencia, "ix_sinistro_data_ocorrencia");

            entity.HasIndex(e => e.EstipulanteId, "ix_sinistro_estipulante");

            entity.HasIndex(e => e.PropostaId, "ix_sinistro_proposta");

            entity.HasIndex(e => e.StatusId, "ix_sinistro_status");

            entity.HasIndex(e => e.ClienteVinculoId, "ix_sinistro_vinculo");

            entity.HasIndex(e => e.LegadoId, "ux_sinistro_legado").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Causa).HasColumnName("causa");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.ClienteVinculoId).HasColumnName("cliente_vinculo_id");
            entity.Property(e => e.CpfSinistradoLimpo)
                .HasMaxLength(20)
                .HasColumnName("cpf_sinistrado_limpo");
            entity.Property(e => e.CpfSinistradoOriginal)
                .HasMaxLength(30)
                .HasColumnName("cpf_sinistrado_original");
            entity.Property(e => e.CpfSinistradoValido).HasColumnName("cpf_sinistrado_valido");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DataAviso).HasColumnName("data_aviso");
            entity.Property(e => e.DataCarta).HasColumnName("data_carta");
            entity.Property(e => e.DataEncerramento).HasColumnName("data_encerramento");
            entity.Property(e => e.DataEnvioSeguradora).HasColumnName("data_envio_seguradora");
            entity.Property(e => e.DataOcorrencia).HasColumnName("data_ocorrencia");
            entity.Property(e => e.DataProtocolo).HasColumnName("data_protocolo");
            entity.Property(e => e.DataRegulacao).HasColumnName("data_regulacao");
            entity.Property(e => e.DataRelacaoFamilia).HasColumnName("data_relacao_familia");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.EstipulanteId).HasColumnName("estipulante_id");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.NumeroSinistro)
                .HasMaxLength(80)
                .HasColumnName("numero_sinistro");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.PropostaId).HasColumnName("proposta_id");
            entity.Property(e => e.PublicId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("public_id");
            entity.Property(e => e.SeguradoraId).HasColumnName("seguradora_id");
            entity.Property(e => e.SituacaoOriginal)
                .HasMaxLength(80)
                .HasColumnName("situacao_original");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.TipoPlanoLegadoId).HasColumnName("tipo_plano_legado_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.ValorAuxilioFuneral)
                .HasPrecision(18, 2)
                .HasColumnName("valor_auxilio_funeral");
            entity.Property(e => e.ValorAvisado)
                .HasPrecision(18, 2)
                .HasColumnName("valor_avisado");
            entity.Property(e => e.ValorCestaBasica)
                .HasPrecision(18, 2)
                .HasColumnName("valor_cesta_basica");
            entity.Property(e => e.ValorImportancia)
                .HasPrecision(18, 2)
                .HasColumnName("valor_importancia");
            entity.Property(e => e.ValorIndenizacao)
                .HasPrecision(18, 2)
                .HasColumnName("valor_indenizacao");

            entity.HasOne(d => d.Status).WithMany(p => p.Sinistros)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("sinistro_status_id_fkey");
        });

        modelBuilder.Entity<SinistroBeneficiario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sinistro_beneficiario_pkey");

            entity.ToTable("sinistro_beneficiario", "sinistro");

            entity.HasIndex(e => e.CpfLimpo, "ix_sinistro_beneficiario_cpf");

            entity.HasIndex(e => e.PropostaBeneficiarioId, "ix_sinistro_beneficiario_proposta_beneficiario");

            entity.HasIndex(e => e.SinistroId, "ix_sinistro_beneficiario_sinistro");

            entity.HasIndex(e => e.LegadoId, "ux_sinistro_beneficiario_legado")
                .IsUnique()
                .HasFilter("(legado_id IS NOT NULL)");

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
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.Nome)
                .HasMaxLength(150)
                .HasColumnName("nome");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.ParentescoOriginal)
                .HasMaxLength(100)
                .HasColumnName("parentesco_original");
            entity.Property(e => e.PercentualParticipacao)
                .HasPrecision(10, 4)
                .HasColumnName("percentual_participacao");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.PropostaBeneficiarioId).HasColumnName("proposta_beneficiario_id");
            entity.Property(e => e.PropostaId).HasColumnName("proposta_id");
            entity.Property(e => e.SinistroId).HasColumnName("sinistro_id");
            entity.Property(e => e.ValorPago)
                .HasPrecision(18, 2)
                .HasColumnName("valor_pago");

            entity.HasOne(d => d.Sinistro).WithMany(p => p.SinistroBeneficiarios)
                .HasForeignKey(d => d.SinistroId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sinistro_beneficiario_sinistro_id_fkey");
        });

        modelBuilder.Entity<SinistroCobertura>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sinistro_cobertura_pkey");

            entity.ToTable("sinistro_cobertura", "sinistro");

            entity.HasIndex(e => e.CoberturaId, "ix_sinistro_cobertura_cobertura");

            entity.HasIndex(e => e.PropostaCoberturaId, "ix_sinistro_cobertura_proposta_cobertura");

            entity.HasIndex(e => e.SinistroId, "ix_sinistro_cobertura_sinistro");

            entity.HasIndex(e => e.CoberturaSinistroLegadoId, "ux_sinistro_cobertura_cobertura_legado")
                .IsUnique()
                .HasFilter("(cobertura_sinistro_legado_id IS NOT NULL)");

            entity.HasIndex(e => e.LegadoId, "ux_sinistro_cobertura_legado")
                .IsUnique()
                .HasFilter("(legado_id IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CoberturaId).HasColumnName("cobertura_id");
            entity.Property(e => e.CoberturaSinistroLegadoId).HasColumnName("cobertura_sinistro_legado_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.PremioConjuge)
                .HasPrecision(18, 2)
                .HasColumnName("premio_conjuge");
            entity.Property(e => e.PremioTitular)
                .HasPrecision(18, 2)
                .HasColumnName("premio_titular");
            entity.Property(e => e.PropostaCoberturaId).HasColumnName("proposta_cobertura_id");
            entity.Property(e => e.PropostaId).HasColumnName("proposta_id");
            entity.Property(e => e.SinistroId).HasColumnName("sinistro_id");
            entity.Property(e => e.ValorEstimado)
                .HasPrecision(18, 2)
                .HasColumnName("valor_estimado");
            entity.Property(e => e.ValorPago)
                .HasPrecision(18, 2)
                .HasColumnName("valor_pago");

            entity.HasOne(d => d.Sinistro).WithMany(p => p.SinistroCoberturas)
                .HasForeignKey(d => d.SinistroId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sinistro_cobertura_sinistro_id_fkey");
        });

        modelBuilder.Entity<SinistroStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sinistro_status_pkey");

            entity.ToTable("sinistro_status", "sinistro");

            entity.HasIndex(e => e.Codigo, "sinistro_status_codigo_key").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.Codigo)
                .HasMaxLength(40)
                .HasColumnName("codigo");
            entity.Property(e => e.Finalizador).HasColumnName("finalizador");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
