using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Cadastro.Domain;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Configurations;

internal sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("cliente", "cadastro");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.PublicId).HasColumnName("public_id");
        builder.Property(c => c.PessoaId).HasColumnName("pessoa_id");
        builder.Property(c => c.StatusId).HasColumnName("status_id");
        builder.Property(c => c.Falecido).HasColumnName("falecido");
        builder.Property(c => c.DataObito).HasColumnName("data_obito");
        builder.Property(c => c.Observacao).HasColumnName("observacao");
        builder.Property(c => c.DataCadastroLegado).HasColumnName("data_cadastro_legado");
        builder.Property(c => c.LegadoId).HasColumnName("legado_id");
        builder.Property(c => c.Re).HasColumnName("re").HasMaxLength(32).IsRequired(false);
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");
        builder.Property(c => c.DeletedAt).HasColumnName("deleted_at");
    }
}

internal sealed class ClienteStatusConfiguration : IEntityTypeConfiguration<ClienteStatusModel>
{
    public void Configure(EntityTypeBuilder<ClienteStatusModel> builder)
    {
        builder.ToTable("cliente_status", "cadastro");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.Codigo).HasColumnName("codigo");
        builder.Property(s => s.Nome).HasColumnName("nome");
        builder.Property(s => s.Ativo).HasColumnName("ativo");
    }
}

internal sealed class ClienteVinculoConfiguration : IEntityTypeConfiguration<ClienteVinculoModel>
{
    public void Configure(EntityTypeBuilder<ClienteVinculoModel> builder)
    {
        builder.ToTable("cliente_vinculo", "cadastro");
        builder.HasKey(v => v.Id);
    }
}

internal sealed class ClienteDependenteConfiguration : IEntityTypeConfiguration<ClienteDependenteModel>
{
    public void Configure(EntityTypeBuilder<ClienteDependenteModel> builder)
    {
        builder.ToTable("cliente_dependente", "cadastro");
        builder.HasKey(d => d.Id);
    }
}
