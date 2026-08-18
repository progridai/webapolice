using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Cadastro.Domain;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Configurations;

public class AgenciadorDomainConfiguration : IEntityTypeConfiguration<Agenciador>
{
    public void Configure(EntityTypeBuilder<Agenciador> builder)
    {
        builder.ToTable("agenciador", "cadastro", t => t.ExcludeFromMigrations());

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PublicId).HasColumnName("public_id");
        builder.Property(x => x.PessoaId).HasColumnName("pessoa_id");
        builder.Property(x => x.CidadeId).HasColumnName("cidade_id");
        builder.Property(x => x.BancoId).HasColumnName("banco_id");
        builder.Property(x => x.CoordenadorId).HasColumnName("coordenador_id");
        
        builder.Property(x => x.Nome).HasColumnName("nome");
        builder.Property(x => x.CpfValido).HasColumnName("cpf_valido");
        
        builder.Property(x => x.Codigo).HasColumnName("codigo");
        builder.Property(x => x.Tipo).HasColumnName("tipo").HasConversion<short>();
        builder.Property(x => x.Susep).HasColumnName("susep");
        builder.Property(x => x.Inss).HasColumnName("inss");
        builder.Property(x => x.Issqn).HasColumnName("issqn");
        builder.Property(x => x.NumeroDependentes).HasColumnName("numero_dependentes");
        builder.Property(x => x.DataInscricao).HasColumnName("data_inscricao");
        builder.Property(x => x.Credenciado).HasColumnName("credenciado");
        
        builder.Property(x => x.Agencia).HasColumnName("agencia");
        builder.Property(x => x.ContaCorrente).HasColumnName("conta_corrente");
        builder.Property(x => x.Observacao).HasColumnName("observacao");
        builder.Property(x => x.LegadoId).HasColumnName("legado_id");
        
        builder.Property(x => x.Desativado).HasColumnName("desativado");
        builder.Property(x => x.DataDesativado).HasColumnName("data_desativado");
        
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
    }
}
