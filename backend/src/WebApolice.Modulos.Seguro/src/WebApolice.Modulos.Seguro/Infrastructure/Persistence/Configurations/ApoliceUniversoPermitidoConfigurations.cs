using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Configurations;

public class ApoliceProdutoConfiguration : IEntityTypeConfiguration<ApoliceProdutoModel>
{
    public void Configure(EntityTypeBuilder<ApoliceProdutoModel> builder)
    {
        builder.ToTable("apolice_produto", "seguro");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(x => x.ApoliceId).HasColumnName("apolice_id").IsRequired();
        builder.Property(x => x.ProdutoId).HasColumnName("produto_id").IsRequired();
        
        builder.Property(x => x.Ativo).HasColumnName("ativo").HasDefaultValue(true).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();

        // FK Apolice
        builder.HasOne(x => x.Apolice).WithMany(x => x.ApoliceProdutos).HasForeignKey(x => x.ApoliceId).OnDelete(DeleteBehavior.Cascade);
        
        // FK Mestre Produto
        builder.HasOne(x => x.Produto).WithMany().HasForeignKey(x => x.ProdutoId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ApolicePlanoConfiguration : IEntityTypeConfiguration<ApolicePlanoModel>
{
    public void Configure(EntityTypeBuilder<ApolicePlanoModel> builder)
    {
        builder.ToTable("apolice_plano", "seguro");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(x => x.ApoliceProdutoId).HasColumnName("apolice_produto_id").IsRequired();
        builder.Property(x => x.PlanoId).HasColumnName("plano_id").IsRequired();
        builder.Property(x => x.TabelaPrecoId).HasColumnName("tabela_preco_id");
        
        builder.Property(x => x.Ativo).HasColumnName("ativo").HasDefaultValue(true).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();

        // FK ApoliceProduto
        builder.HasOne(x => x.ApoliceProduto).WithMany(x => x.Planos).HasForeignKey(x => x.ApoliceProdutoId).OnDelete(DeleteBehavior.Cascade);
        
        // FK Mestre Plano / Tabela
        builder.HasOne(x => x.Plano).WithMany().HasForeignKey(x => x.PlanoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.TabelaPreco).WithMany().HasForeignKey(x => x.TabelaPrecoId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class ApoliceCoberturaConfiguration : IEntityTypeConfiguration<ApoliceCoberturaModel>
{
    public void Configure(EntityTypeBuilder<ApoliceCoberturaModel> builder)
    {
        builder.ToTable("apolice_cobertura", "seguro");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(x => x.ApolicePlanoId).HasColumnName("apolice_plano_id").IsRequired();
        builder.Property(x => x.CoberturaId).HasColumnName("cobertura_id").IsRequired();
        
        builder.Property(x => x.ImportanciaSeguradaOverride).HasColumnName("importancia_segurada_override").HasPrecision(18, 2);
        builder.Property(x => x.PremioOverride).HasColumnName("premio_override").HasPrecision(18, 2);
        
        builder.Property(x => x.Ativo).HasColumnName("ativo").HasDefaultValue(true).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();

        // FK ApolicePlano
        builder.HasOne(x => x.ApolicePlano).WithMany(x => x.Coberturas).HasForeignKey(x => x.ApolicePlanoId).OnDelete(DeleteBehavior.Cascade);
        
        // FK Mestre Cobertura
        builder.HasOne(x => x.Cobertura).WithMany().HasForeignKey(x => x.CoberturaId).OnDelete(DeleteBehavior.Restrict);
    }
}
