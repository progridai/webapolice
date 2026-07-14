using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApolice.Modulos.Clientes.Infrastructure.Persistence.Models;
using WebApolice.Modulos.Clientes.Infrastructure.Persistence.Models.Vinculos;

namespace WebApolice.Modulos.Clientes.Infrastructure.Persistence.Configurations;

internal static class InfrastructureModelsConfiguration
{
    public static void ApplyConfigurations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PessoaModel>(builder =>
        {
            builder.ToTable("pessoa", "core", t => t.ExcludeFromMigrations());
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.PublicId).HasColumnName("public_id");
            builder.Property(x => x.TipoPessoa).HasColumnName("tipo_pessoa");
            builder.Property(x => x.Nome).HasColumnName("nome");
            builder.Property(x => x.NomeNormalizado).HasColumnName("nome_normalizado");
            builder.Property(x => x.DocumentoPrincipal).HasColumnName("documento_principal");
            builder.Property(x => x.DocumentoPrincipalLimpo).HasColumnName("documento_principal_limpo");
            builder.Property(x => x.DocumentoValido).HasColumnName("documento_valido");
            builder.Property(x => x.DataNascimento).HasColumnName("data_nascimento");
            builder.Property(x => x.Sexo).HasColumnName("sexo");
            builder.Property(x => x.Observacao).HasColumnName("observacao");
            builder.Property(x => x.CreatedAt).HasColumnName("created_at");
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        });

        modelBuilder.Entity<PessoaContatoModel>(builder =>
        {
            builder.ToTable("pessoa_contato", "core", t => t.ExcludeFromMigrations());
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.PessoaId).HasColumnName("pessoa_id");
            builder.Property(x => x.TipoContato).HasColumnName("tipo_contato");
            builder.Property(x => x.Valor).HasColumnName("valor");
            builder.Property(x => x.ValorNormalizado).HasColumnName("valor_normalizado");
            builder.Property(x => x.Principal).HasColumnName("principal");
            builder.Property(x => x.Ativo).HasColumnName("ativo");
            builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<PessoaEnderecoModel>(builder =>
        {
            builder.ToTable("pessoa_endereco", "core", t => t.ExcludeFromMigrations());
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.PessoaId).HasColumnName("pessoa_id");
            builder.Property(x => x.CidadeId).HasColumnName("cidade_id");
            builder.Property(x => x.TipoEndereco).HasColumnName("tipo_endereco");
            builder.Property(x => x.Cep).HasColumnName("cep");
            builder.Property(x => x.Logradouro).HasColumnName("logradouro");
            builder.Property(x => x.Numero).HasColumnName("numero");
            builder.Property(x => x.Complemento).HasColumnName("complemento");
            builder.Property(x => x.Bairro).HasColumnName("bairro");
            builder.Property(x => x.Uf).HasColumnName("uf");
            builder.Property(x => x.Principal).HasColumnName("principal");
            builder.Property(x => x.Ativo).HasColumnName("ativo");
            builder.Property(x => x.LegadoSituacaoEndereco).HasColumnName("legado_situacao_endereco");
            builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<ClienteStatusModel>(builder =>
        {
            builder.ToTable("cliente_status", "cadastro", t => t.ExcludeFromMigrations());
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Codigo).HasColumnName("codigo");
            builder.Property(x => x.Nome).HasColumnName("nome");
            builder.Property(x => x.Ativo).HasColumnName("ativo");
        });

        modelBuilder.Entity<ClienteVinculoModel>(builder =>
        {
            builder.ToTable("cliente_vinculo", "cadastro", t => t.ExcludeFromMigrations());
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.ClienteId).HasColumnName("cliente_id");
            builder.Property(x => x.PessoaId).HasColumnName("pessoa_id");
            builder.Property(x => x.EstipulanteId).HasColumnName("estipulante_id");
            builder.Property(x => x.SubestipulanteId).HasColumnName("subestipulante_id");
            builder.Property(x => x.GrupoId).HasColumnName("grupo_id");
            builder.Property(x => x.SubgrupoId).HasColumnName("subgrupo_id");
            builder.Property(x => x.LotacaoId).HasColumnName("lotacao_id");
            builder.Property(x => x.Matricula).HasColumnName("matricula");
            builder.Property(x => x.BancoId).HasColumnName("banco_id");
            builder.Property(x => x.Agencia).HasColumnName("agencia");
            builder.Property(x => x.ContaCorrente).HasColumnName("conta_corrente");
            builder.Property(x => x.Ativo).HasColumnName("ativo");
            builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<ClienteDependenteModel>(builder =>
        {
            builder.ToTable("cliente_dependente", "cadastro", t => t.ExcludeFromMigrations());
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.ClienteId).HasColumnName("cliente_id");
            builder.Property(x => x.PessoaId).HasColumnName("pessoa_id");
            builder.Property(x => x.TipoRelacao).HasColumnName("tipo_relacao");
            builder.Property(x => x.Nome).HasColumnName("nome");
            builder.Property(x => x.Cpf).HasColumnName("cpf");
            builder.Property(x => x.DataNascimento).HasColumnName("data_nascimento");
            builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<EstipulanteModel>(builder =>
        {
            builder.ToTable("estipulante", "cadastro", t => t.ExcludeFromMigrations());
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.PublicId).HasColumnName("public_id");
            builder.Property(x => x.PessoaId).HasColumnName("pessoa_id");
            builder.Property(x => x.Nome).HasColumnName("nome");
            builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        });

        modelBuilder.Entity<SubestipulanteModel>(builder =>
        {
            builder.ToTable("subestipulante", "cadastro", t => t.ExcludeFromMigrations());
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.PublicId).HasColumnName("public_id");
            builder.Property(x => x.PessoaId).HasColumnName("pessoa_id");
            builder.Property(x => x.Nome).HasColumnName("nome");
            builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        });

        modelBuilder.Entity<CorretoraModel>(builder =>
        {
            builder.ToTable("corretora", "cadastro", t => t.ExcludeFromMigrations());
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.PessoaId).HasColumnName("pessoa_id");
            builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        });

        modelBuilder.Entity<SeguradoraModel>(builder =>
        {
            builder.ToTable("seguradora", "cadastro", t => t.ExcludeFromMigrations());
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.PessoaId).HasColumnName("pessoa_id");
            builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        });

        modelBuilder.Entity<AgenciadorModel>(builder =>
        {
            builder.ToTable("agenciador", "cadastro", t => t.ExcludeFromMigrations());
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.PessoaId).HasColumnName("pessoa_id");
            builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        });

        modelBuilder.Entity<GrupoModel>(builder =>
        {
            builder.ToTable("grupo", "cadastro", t => t.ExcludeFromMigrations());
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Nome).HasColumnName("nome");
        });

        modelBuilder.Entity<SubgrupoModel>(builder =>
        {
            builder.ToTable("subgrupo", "cadastro", t => t.ExcludeFromMigrations());
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Nome).HasColumnName("nome");
        });

        modelBuilder.Entity<LotacaoModel>(builder =>
        {
            builder.ToTable("lotacao", "cadastro", t => t.ExcludeFromMigrations());
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Nome).HasColumnName("nome");
        });

        modelBuilder.Entity<BancoModel>(builder =>
        {
            builder.ToTable("banco", "core", t => t.ExcludeFromMigrations());
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Codigo).HasColumnName("codigo");
            builder.Property(x => x.Nome).HasColumnName("nome");
        });
    }
}
