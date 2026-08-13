using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Clientes.Domain;

namespace WebApolice.Modulos.Clientes.Infrastructure.Persistence.Configurations;

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
