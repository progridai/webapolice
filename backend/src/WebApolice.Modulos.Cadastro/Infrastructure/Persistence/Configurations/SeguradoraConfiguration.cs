using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Configurations;

public class SeguradoraConfiguration : IEntityTypeConfiguration<SeguradoraModel>
{
    public void Configure(EntityTypeBuilder<SeguradoraModel> builder)
    {
        builder.ToTable("seguradora", "cadastro");
        builder.HasKey(x => x.Id).HasName("pk_seguradora");

        builder.Property(x => x.Id).HasColumnName("id").IsRequired();
        builder.Property(x => x.PublicId).HasColumnName("public_id").IsRequired();
        builder.Property(x => x.PessoaId).HasColumnName("pessoa_id").IsRequired();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50);
        builder.Property(x => x.Susep).HasColumnName("susep").HasMaxLength(50);
        
        builder.Property(x => x.Ativo).HasColumnName("ativo").IsRequired().HasDefaultValue(true);
        builder.Property(x => x.Observacao).HasColumnName("observacao");
        builder.Property(x => x.LegadoId).HasColumnName("legado_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(x => x.Pessoa)
               .WithMany()
               .HasForeignKey(x => x.PessoaId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
