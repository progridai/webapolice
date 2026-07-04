using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Clientes.Domain;

namespace WebApolice.Modulos.Clientes.Infrastructure.Persistence.Configurations;

internal sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes", "clientes", t => 
        {
            t.HasCheckConstraint("ck_clientes_status", "status IN ('Ativo', 'Inativo')");
        });

        builder.HasKey(c => c.Id).HasName("pk_clientes");

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(c => c.Nome)
            .HasColumnName("nome")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(c => c.Cpf)
            .HasColumnName("cpf")
            .HasMaxLength(11)
            .IsRequired();

        builder.Property(c => c.DataNascimento)
            .HasColumnName("data_nascimento");

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(254);

        builder.Property(c => c.Telefone)
            .HasColumnName("telefone")
            .HasMaxLength(20);

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.DataCadastroUtc)
            .HasColumnName("data_cadastro_utc")
            .IsRequired();

        builder.Property(c => c.DataAtualizacaoUtc)
            .HasColumnName("data_atualizacao_utc")
            .IsRequired();

        builder.Property(c => c.CodigoLegado)
            .HasColumnName("codigo_legado");

        // Índices
        builder.HasIndex(c => c.Cpf)
            .IsUnique()
            .HasDatabaseName("uk_clientes_cpf");

        builder.HasIndex(c => c.Nome)
            .HasDatabaseName("ix_clientes_nome");

        builder.HasIndex(c => c.CodigoLegado)
            .IsUnique()
            .HasDatabaseName("ix_clientes_codigo_legado")
            .HasFilter("codigo_legado IS NOT NULL");
    }
}
