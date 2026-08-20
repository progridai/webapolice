using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Configurations;

public class CorretoraConfiguration : IEntityTypeConfiguration<CorretoraModel>
{
    public void Configure(EntityTypeBuilder<CorretoraModel> builder)
    {
        builder.ToTable("corretora", "cadastro");
        builder.HasKey(x => x.Id).HasName("pk_corretora");

        builder.Property(x => x.Id).HasColumnName("id").IsRequired();
        builder.Property(x => x.PublicId).HasColumnName("public_id").IsRequired();
        builder.Property(x => x.PessoaId).HasColumnName("pessoa_id").IsRequired();
        
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(80);
        builder.Property(x => x.CodigoProtheus).HasColumnName("codigo_protheus").HasMaxLength(50);
        
        builder.Property(x => x.Ativo).HasColumnName("ativo").IsRequired().HasDefaultValue(true);
        builder.Property(x => x.Observacao).HasColumnName("observacao");
        builder.Property(x => x.LegadoId).HasColumnName("legado_id");
        
        builder.Property(x => x.CaminhoLogotipoLegado).HasColumnName("caminho_logotipo_legado").HasMaxLength(300);
        builder.Property(x => x.LogotipoArquivoId).HasColumnName("logotipo_arquivo_id");
        builder.Property(x => x.PossuiLogotipoLegado).HasColumnName("possui_logotipo_legado").IsRequired();
        
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(x => x.Pessoa)
               .WithMany()
               .HasForeignKey(x => x.PessoaId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
