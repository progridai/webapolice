using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Configurations;

public class InfrastructureModelsConfiguration : IEntityTypeConfiguration<PessoaModel>
{
    public void Configure(EntityTypeBuilder<PessoaModel> builder)
    {
        builder.ToTable("pessoa", "core", t => t.ExcludeFromMigrations());

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PublicId).HasColumnName("public_id");
        builder.Property(x => x.TipoPessoa).HasColumnName("tipo_pessoa");
        builder.Property(x => x.Nome).HasColumnName("nome");
        builder.Property(x => x.DocumentoPrincipal).HasColumnName("documento_principal");
    }
}

public class PessoaEnderecoConfiguration : IEntityTypeConfiguration<PessoaEnderecoModel>
{
    public void Configure(EntityTypeBuilder<PessoaEnderecoModel> builder)
    {
        builder.ToTable("pessoa_endereco", "core", t => t.ExcludeFromMigrations());
        builder.HasKey(x => x.Id);
    }
}

public class PessoaContatoConfiguration : IEntityTypeConfiguration<PessoaContatoModel>
{
    public void Configure(EntityTypeBuilder<PessoaContatoModel> builder)
    {
        builder.ToTable("pessoa_contato", "core", t => t.ExcludeFromMigrations());
        builder.HasKey(x => x.Id);
    }
}

public class PessoaDocumentoConfiguration : IEntityTypeConfiguration<PessoaDocumentoModel>
{
    public void Configure(EntityTypeBuilder<PessoaDocumentoModel> builder)
    {
        builder.ToTable("pessoa_documento", "core", t => t.ExcludeFromMigrations());
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PessoaId).HasColumnName("pessoa_id");
        builder.Property(x => x.TipoDocumento).HasColumnName("tipo_documento");
        builder.Property(x => x.Numero).HasColumnName("numero");
        builder.Property(x => x.NumeroLimpo).HasColumnName("numero_limpo");
        builder.Property(x => x.OrgaoEmissor).HasColumnName("orgao_emissor");
        builder.Property(x => x.DataEmissao).HasColumnName("data_emissao");
        builder.Property(x => x.Principal).HasColumnName("principal");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
    }
}

public class PessoaContatoInstitucionalConfiguration : IEntityTypeConfiguration<PessoaContatoInstitucionalModel>
{
    public void Configure(EntityTypeBuilder<PessoaContatoInstitucionalModel> builder)
    {
        builder.ToTable("pessoa_contato_institucional", "core", t => t.ExcludeFromMigrations());
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PessoaId).HasColumnName("pessoa_id");
        builder.Property(x => x.Nome).HasColumnName("nome");
        builder.Property(x => x.Departamento).HasColumnName("departamento");
        builder.Property(x => x.Email).HasColumnName("email");
        builder.Property(x => x.Telefone).HasColumnName("telefone");
        builder.Property(x => x.Ramal).HasColumnName("ramal");
        builder.Property(x => x.Ativo).HasColumnName("ativo");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
    }
}

public class GrupoConfiguration : IEntityTypeConfiguration<GrupoModel>
{
    public void Configure(EntityTypeBuilder<GrupoModel> builder)
    {
        builder.ToTable("grupo", "cadastro", t => t.ExcludeFromMigrations());
        builder.HasKey(x => x.Id);
    }
}

public class SubgrupoConfiguration : IEntityTypeConfiguration<SubgrupoModel>
{
    public void Configure(EntityTypeBuilder<SubgrupoModel> builder)
    {
        builder.ToTable("subgrupo", "cadastro", t => t.ExcludeFromMigrations());
        builder.HasKey(x => x.Id);
    }
}

public class LotacaoConfiguration : IEntityTypeConfiguration<LotacaoModel>
{
    public void Configure(EntityTypeBuilder<LotacaoModel> builder)
    {
        builder.ToTable("lotacao", "cadastro", t => t.ExcludeFromMigrations());
        builder.HasKey(x => x.Id);
    }
}

public class BancoConfiguration : IEntityTypeConfiguration<BancoModel>
{
    public void Configure(EntityTypeBuilder<BancoModel> builder)
    {
        builder.ToTable("banco", "core", t => t.ExcludeFromMigrations());
        builder.HasKey(x => x.Id);
    }
}



public class CidadeConfiguration : IEntityTypeConfiguration<CidadeModel>
{
    public void Configure(EntityTypeBuilder<CidadeModel> builder)
    {
        builder.ToTable("cidade", "core", t => t.ExcludeFromMigrations());
        builder.HasKey(x => x.Id);
    }
}
