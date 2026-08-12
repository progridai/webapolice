using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Estipulantes.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Estipulantes.Infrastructure.Persistence.Configurations;

public class EstipulanteConfiguration : IEntityTypeConfiguration<EstipulanteModel>
{
    public void Configure(EntityTypeBuilder<EstipulanteModel> builder)
    {
        builder.ToTable("estipulante", "cadastro", t => t.ExcludeFromMigrations());
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PublicId).HasColumnName("public_id");
        builder.Property(x => x.PessoaId).HasColumnName("pessoa_id");
        builder.Property(x => x.Nome).HasColumnName("nome");
        builder.Property(x => x.NomeFormatado).HasColumnName("nome_formatado");
        builder.Property(x => x.Codigo).HasColumnName("codigo");
        builder.Property(x => x.TipoPessoa).HasColumnName("tipo_pessoa");
        builder.Property(x => x.Cnpj).HasColumnName("cnpj");
        builder.Property(x => x.CnpjLimpo).HasColumnName("cnpj_limpo");
        builder.Property(x => x.CidadeId).HasColumnName("cidade_id");
        builder.Property(x => x.GrupoId).HasColumnName("grupo_id");
        builder.Property(x => x.SeguradoraId).HasColumnName("seguradora_id");
        builder.Property(x => x.Ativo).HasColumnName("ativo");
        builder.Property(x => x.Observacao).HasColumnName("observacao");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(x => x.Pessoa)
            .WithMany(p => p.Estipulantes)
            .HasForeignKey(x => x.PessoaId);
            
        builder.HasOne(x => x.Configuracao)
            .WithOne(c => c.Estipulante)
            .HasForeignKey<EstipulanteConfiguracaoModel>(c => c.EstipulanteId);
    }
}
