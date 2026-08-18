using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Convenio.src.WebApolice.Modulos.Convenio.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Convenio.src.WebApolice.Modulos.Convenio.Infrastructure.Persistence;

public partial class ConvenioDbContext : DbContext
{
    public ConvenioDbContext()
    {
    }

    public ConvenioDbContext(DbContextOptions<ConvenioDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CorsanCliente> CorsanClientes { get; set; }

    public virtual DbSet<CorsanPropostum> CorsanProposta { get; set; }

    public virtual DbSet<SiapeCliente> SiapeClientes { get; set; }

    public virtual DbSet<SiapeOrgao> SiapeOrgaos { get; set; }

    public virtual DbSet<SiapeParametro> SiapeParametros { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=painel.bravida.com.br;Port=5432;Database=webapolice_teste;Username=bravito;Password=Bravida@2023!");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("unaccent");

        modelBuilder.Entity<CorsanCliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("corsan_cliente_pkey");

            entity.ToTable("corsan_cliente", "convenio");

            entity.HasIndex(e => e.ClienteId, "ix_corsan_cliente_cliente");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.ClienteVinculoId).HasColumnName("cliente_vinculo_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Empresa)
                .HasMaxLength(100)
                .HasColumnName("empresa");
            entity.Property(e => e.Funcionario).HasColumnName("funcionario");
            entity.Property(e => e.Grupo)
                .HasMaxLength(100)
                .HasColumnName("grupo");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.Rubrica)
                .HasMaxLength(100)
                .HasColumnName("rubrica");
        });

        modelBuilder.Entity<CorsanPropostum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("corsan_proposta_pkey");

            entity.ToTable("corsan_proposta", "convenio");

            entity.HasIndex(e => e.ClienteId, "ix_corsan_proposta_cliente");

            entity.HasIndex(e => e.PropostaId, "ux_corsan_proposta").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.ClienteVinculoId).HasColumnName("cliente_vinculo_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Empresa)
                .HasMaxLength(20)
                .HasColumnName("empresa");
            entity.Property(e => e.Grupo)
                .HasMaxLength(20)
                .HasColumnName("grupo");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.PropostaId).HasColumnName("proposta_id");
            entity.Property(e => e.Rubrica)
                .HasMaxLength(20)
                .HasColumnName("rubrica");
        });

        modelBuilder.Entity<SiapeCliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("siape_cliente_pkey");

            entity.ToTable("siape_cliente", "convenio");

            entity.HasIndex(e => e.ClienteId, "ix_siape_cliente_cliente");

            entity.HasIndex(e => e.Siape, "ix_siape_cliente_siape");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Agencia)
                .HasMaxLength(30)
                .HasColumnName("agencia");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.Categoria)
                .HasMaxLength(30)
                .HasColumnName("categoria");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.ClienteVinculoId).HasColumnName("cliente_vinculo_id");
            entity.Property(e => e.Contrato)
                .HasMaxLength(100)
                .HasColumnName("contrato");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DigitoVerificador)
                .HasMaxLength(10)
                .HasColumnName("digito_verificador");
            entity.Property(e => e.Funcao)
                .HasMaxLength(30)
                .HasColumnName("funcao");
            entity.Property(e => e.Instituidor)
                .HasMaxLength(100)
                .HasColumnName("instituidor");
            entity.Property(e => e.Instituto)
                .HasMaxLength(30)
                .HasColumnName("instituto");
            entity.Property(e => e.OrgaoId).HasColumnName("orgao_id");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.Setor)
                .HasMaxLength(30)
                .HasColumnName("setor");
            entity.Property(e => e.Siape)
                .HasMaxLength(100)
                .HasColumnName("siape");

            entity.HasOne(d => d.Orgao).WithMany(p => p.SiapeClientes)
                .HasForeignKey(d => d.OrgaoId)
                .HasConstraintName("siape_cliente_orgao_id_fkey");
        });

        modelBuilder.Entity<SiapeOrgao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("siape_orgao_pkey");

            entity.ToTable("siape_orgao", "convenio");

            entity.HasIndex(e => e.LegadoId, "ux_siape_orgao_legado")
                .IsUnique()
                .HasFilter("(legado_id IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Codigo)
                .HasMaxLength(50)
                .HasColumnName("codigo");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.Nome)
                .HasMaxLength(150)
                .HasColumnName("nome");
        });

        modelBuilder.Entity<SiapeParametro>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("siape_parametro_pkey");

            entity.ToTable("siape_parametro", "convenio");

            entity.HasIndex(e => e.LegadoId, "ux_siape_parametro_legado")
                .IsUnique()
                .HasFilter("(legado_id IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.CalculoParametro)
                .HasMaxLength(50)
                .HasColumnName("calculo_parametro");
            entity.Property(e => e.Cgc)
                .HasMaxLength(30)
                .HasColumnName("cgc");
            entity.Property(e => e.CgcLimpo)
                .HasMaxLength(20)
                .HasColumnName("cgc_limpo");
            entity.Property(e => e.Comando)
                .HasMaxLength(50)
                .HasColumnName("comando");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CustoLinha)
                .HasPrecision(18, 2)
                .HasColumnName("custo_linha");
            entity.Property(e => e.Empresa)
                .HasMaxLength(100)
                .HasColumnName("empresa");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.Rubrica)
                .HasMaxLength(50)
                .HasColumnName("rubrica");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
