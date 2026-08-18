using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Domain;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Configurations;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence;

public class CadastroDbContext : DbContext
{
    public CadastroDbContext(DbContextOptions<CadastroDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<PessoaModel> Pessoas => Set<PessoaModel>();
    public DbSet<PessoaContatoModel> Contatos => Set<PessoaContatoModel>();
    public DbSet<PessoaEnderecoModel> Enderecos => Set<PessoaEnderecoModel>();
    public DbSet<PessoaDocumentoModel> Documentos => Set<PessoaDocumentoModel>();
    public DbSet<PessoaContatoInstitucionalModel> ContatosInstitucionais => Set<PessoaContatoInstitucionalModel>();
    public DbSet<ClienteStatusModel> Status => Set<ClienteStatusModel>();
    public DbSet<ClienteVinculoModel> Vinculos => Set<ClienteVinculoModel>();
    public DbSet<ClienteDependenteModel> Dependentes => Set<ClienteDependenteModel>();
    
    public DbSet<EstipulanteModel> Estipulantes => Set<EstipulanteModel>();
    public DbSet<EstipulanteConfiguracaoModel> EstipulanteConfiguracoes => Set<EstipulanteConfiguracaoModel>();
    public DbSet<SubestipulanteModel> Subestipulantes => Set<SubestipulanteModel>();
    public DbSet<ModuloModel> Modulos => Set<ModuloModel>();
    public DbSet<CorretoraModel> Corretoras => Set<CorretoraModel>();
    public DbSet<SeguradoraModel> Seguradoras => Set<SeguradoraModel>();
    public DbSet<Agenciador> Agenciadores => Set<Agenciador>();
    public DbSet<GrupoModel> Grupos => Set<GrupoModel>();
    public DbSet<SubgrupoModel> Subgrupos => Set<SubgrupoModel>();
    public DbSet<LotacaoModel> Lotacoes => Set<LotacaoModel>();
    public DbSet<BancoModel> Bancos => Set<BancoModel>();
    public DbSet<CidadeModel> Cidades => Set<CidadeModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CadastroDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }

    public virtual Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Database.BeginTransactionAsync(cancellationToken);
    }
}
