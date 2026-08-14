using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApolice.Shared.Infrastructure.Persistence.Models;

namespace WebApolice.Shared.Infrastructure.Persistence;

public partial class CoreDbContext : DbContext
{
    public CoreDbContext()
    {
    }

    public CoreDbContext(DbContextOptions<CoreDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Banco> Bancos { get; set; }

    public virtual DbSet<Cidade> Cidades { get; set; }

    public virtual DbSet<Estado> Estados { get; set; }

    public virtual DbSet<Pessoa> Pessoas { get; set; }

    public virtual DbSet<PessoaContato> PessoaContatos { get; set; }

    public virtual DbSet<PessoaContatoInstitucional> PessoaContatoInstitucionals { get; set; }

    public virtual DbSet<PessoaDocumento> PessoaDocumentos { get; set; }

    public virtual DbSet<PessoaEndereco> PessoaEnderecos { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("unaccent");

        modelBuilder.Entity<Banco>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("banco_pkey");

            entity.ToTable("banco", "core");

            entity.HasIndex(e => e.Codigo, "ix_banco_codigo");

            entity.HasIndex(e => e.LegadoId, "ux_banco_legado")
                .IsUnique()
                .HasFilter("(legado_id IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .HasColumnName("codigo");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
        });

        modelBuilder.Entity<Cidade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cidade_pkey");

            entity.ToTable("cidade", "core");

            entity.HasIndex(e => e.Nome, "ix_cidade_nome_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.LegadoId, "ux_cidade_legado")
                .IsUnique()
                .HasFilter("(legado_id IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EstadoId).HasColumnName("estado_id");
            entity.Property(e => e.LegadoId).HasColumnName("legado_id");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");
            entity.Property(e => e.NomeNormalizado)
                .HasMaxLength(100)
                .HasColumnName("nome_normalizado");
            entity.Property(e => e.Uf)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("uf");

            entity.HasOne(d => d.Estado).WithMany(p => p.Cidades)
                .HasForeignKey(d => d.EstadoId)
                .HasConstraintName("cidade_estado_id_fkey");
        });

        modelBuilder.Entity<Estado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("estado_pkey");

            entity.ToTable("estado", "core");

            entity.HasIndex(e => e.Uf, "estado_uf_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");
            entity.Property(e => e.Uf)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("uf");
        });

        modelBuilder.Entity<Pessoa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pessoa_pkey");

            entity.ToTable("pessoa", "core");

            entity.HasIndex(e => e.DocumentoPrincipalLimpo, "ix_pessoa_documento_limpo");

            entity.HasIndex(e => e.DocumentoValido, "ix_pessoa_documento_valido");

            entity.HasIndex(e => e.Nome, "ix_pessoa_nome_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DataNascimento).HasColumnName("data_nascimento");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DocumentoPrincipal)
                .HasMaxLength(30)
                .HasColumnName("documento_principal");
            entity.Property(e => e.DocumentoPrincipalLimpo)
                .HasMaxLength(20)
                .HasColumnName("documento_principal_limpo");
            entity.Property(e => e.DocumentoValido).HasColumnName("documento_valido");
            entity.Property(e => e.Nome)
                .HasMaxLength(150)
                .HasColumnName("nome");
            entity.Property(e => e.NomeNormalizado)
                .HasMaxLength(150)
                .HasColumnName("nome_normalizado");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.PublicId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("public_id");
            entity.Property(e => e.Sexo).HasColumnName("sexo");
            entity.Property(e => e.TipoPessoa)
                .HasDefaultValue((short)1)
                .HasColumnName("tipo_pessoa");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<PessoaContato>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pessoa_contato_pkey");

            entity.ToTable("pessoa_contato", "core");

            entity.HasIndex(e => e.PessoaId, "ix_pessoa_contato_pessoa");

            entity.HasIndex(e => e.TipoContato, "ix_pessoa_contato_tipo");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.Principal).HasColumnName("principal");
            entity.Property(e => e.TipoContato)
                .HasMaxLength(30)
                .HasColumnName("tipo_contato");
            entity.Property(e => e.Valor)
                .HasMaxLength(150)
                .HasColumnName("valor");
            entity.Property(e => e.ValorNormalizado)
                .HasMaxLength(150)
                .HasColumnName("valor_normalizado");

            entity.HasOne(d => d.Pessoa).WithMany(p => p.PessoaContatos)
                .HasForeignKey(d => d.PessoaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pessoa_contato_pessoa_id_fkey");
        });

        modelBuilder.Entity<PessoaContatoInstitucional>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pessoa_contato_institucional_pkey");

            entity.ToTable("pessoa_contato_institucional", "core");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Departamento)
                .HasMaxLength(100)
                .HasColumnName("departamento");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Nome)
                .HasMaxLength(255)
                .HasColumnName("nome");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.Ramal)
                .HasMaxLength(20)
                .HasColumnName("ramal");
            entity.Property(e => e.Telefone)
                .HasMaxLength(50)
                .HasColumnName("telefone");

            entity.HasOne(d => d.Pessoa).WithMany(p => p.PessoaContatoInstitucionals)
                .HasForeignKey(d => d.PessoaId)
                .HasConstraintName("pessoa_contato_institucional_pessoa_id_fkey");
        });

        modelBuilder.Entity<PessoaDocumento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pessoa_documento_pkey");

            entity.ToTable("pessoa_documento", "core");

            entity.HasIndex(e => e.NumeroLimpo, "ix_pessoa_documento_numero");

            entity.HasIndex(e => e.PessoaId, "ix_pessoa_documento_pessoa");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DataEmissao).HasColumnName("data_emissao");
            entity.Property(e => e.Numero)
                .HasMaxLength(50)
                .HasColumnName("numero");
            entity.Property(e => e.NumeroLimpo)
                .HasMaxLength(50)
                .HasColumnName("numero_limpo");
            entity.Property(e => e.OrgaoEmissor)
                .HasMaxLength(50)
                .HasColumnName("orgao_emissor");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.Principal).HasColumnName("principal");
            entity.Property(e => e.TipoDocumento)
                .HasMaxLength(30)
                .HasColumnName("tipo_documento");

            entity.HasOne(d => d.Pessoa).WithMany(p => p.PessoaDocumentos)
                .HasForeignKey(d => d.PessoaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pessoa_documento_pessoa_id_fkey");
        });

        modelBuilder.Entity<PessoaEndereco>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pessoa_endereco_pkey");

            entity.ToTable("pessoa_endereco", "core");

            entity.HasIndex(e => e.PessoaId, "ix_pessoa_endereco_pessoa");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.Bairro)
                .HasMaxLength(100)
                .HasColumnName("bairro");
            entity.Property(e => e.Cep)
                .HasMaxLength(20)
                .HasColumnName("cep");
            entity.Property(e => e.CidadeId).HasColumnName("cidade_id");
            entity.Property(e => e.Complemento)
                .HasMaxLength(150)
                .HasColumnName("complemento");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.LegadoSituacaoEndereco).HasColumnName("legado_situacao_endereco");
            entity.Property(e => e.Logradouro)
                .HasMaxLength(150)
                .HasColumnName("logradouro");
            entity.Property(e => e.Numero)
                .HasMaxLength(30)
                .HasColumnName("numero");
            entity.Property(e => e.PessoaId).HasColumnName("pessoa_id");
            entity.Property(e => e.Principal).HasColumnName("principal");
            entity.Property(e => e.TipoEndereco)
                .HasMaxLength(30)
                .HasDefaultValueSql("'principal'::character varying")
                .HasColumnName("tipo_endereco");
            entity.Property(e => e.Uf)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("uf");

            entity.HasOne(d => d.Cidade).WithMany(p => p.PessoaEnderecos)
                .HasForeignKey(d => d.CidadeId)
                .HasConstraintName("pessoa_endereco_cidade_id_fkey");

            entity.HasOne(d => d.Pessoa).WithMany(p => p.PessoaEnderecos)
                .HasForeignKey(d => d.PessoaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pessoa_endereco_pessoa_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
