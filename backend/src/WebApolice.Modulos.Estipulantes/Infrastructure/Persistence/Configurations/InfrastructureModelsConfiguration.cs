using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Estipulantes.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Estipulantes.Infrastructure.Persistence.Configurations;

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

public class SeguradoraConfiguration : IEntityTypeConfiguration<SeguradoraModel>
{
    public void Configure(EntityTypeBuilder<SeguradoraModel> builder)
    {
        builder.ToTable("seguradora", "cadastro", t => t.ExcludeFromMigrations());
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

public class ClienteConfiguration : IEntityTypeConfiguration<ClienteModel>
{
    public void Configure(EntityTypeBuilder<ClienteModel> builder)
    {
        builder.ToTable("cliente", "cadastro", t => t.ExcludeFromMigrations());
        builder.HasKey(x => x.Id);
    }
}

public class SubestipulanteConfiguration : IEntityTypeConfiguration<SubestipulanteModel>
{
    public void Configure(EntityTypeBuilder<SubestipulanteModel> builder)
    {
        builder.ToTable("subestipulante", "cadastro", t => t.ExcludeFromMigrations());
        builder.HasKey(x => x.Id);
    }
}

public class CorretoraConfiguration : IEntityTypeConfiguration<CorretoraModel>
{
    public void Configure(EntityTypeBuilder<CorretoraModel> builder)
    {
        builder.ToTable("corretora", "cadastro", t => t.ExcludeFromMigrations());
        builder.HasKey(x => x.Id);
    }
}

public class AgenciadorConfiguration : IEntityTypeConfiguration<AgenciadorModel>
{
    public void Configure(EntityTypeBuilder<AgenciadorModel> builder)
    {
        builder.ToTable("agenciador", "cadastro", t => t.ExcludeFromMigrations());
        builder.HasKey(x => x.Id);
    }
}
