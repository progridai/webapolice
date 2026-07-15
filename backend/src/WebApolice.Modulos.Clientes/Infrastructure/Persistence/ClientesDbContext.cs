using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Clientes.Domain;
using WebApolice.Modulos.Clientes.Infrastructure.Persistence.Configurations;
using WebApolice.Modulos.Clientes.Infrastructure.Persistence.Models;
using WebApolice.Modulos.Clientes.Infrastructure.Persistence.Models.Vinculos;

namespace WebApolice.Modulos.Clientes.Infrastructure.Persistence;

public class ClientesDbContext : DbContext
{
    public ClientesDbContext(DbContextOptions<ClientesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<PessoaModel> Pessoas => Set<PessoaModel>();
    public DbSet<PessoaContatoModel> Contatos => Set<PessoaContatoModel>();
    public DbSet<PessoaEnderecoModel> Enderecos => Set<PessoaEnderecoModel>();
    public DbSet<ClienteStatusModel> Status => Set<ClienteStatusModel>();
    public DbSet<ClienteVinculoModel> Vinculos => Set<ClienteVinculoModel>();
    public DbSet<ClienteDependenteModel> Dependentes => Set<ClienteDependenteModel>();
    
    public DbSet<EstipulanteModel> Estipulantes => Set<EstipulanteModel>();
    public DbSet<SubestipulanteModel> Subestipulantes => Set<SubestipulanteModel>();
    public DbSet<CorretoraModel> Corretoras => Set<CorretoraModel>();
    public DbSet<SeguradoraModel> Seguradoras => Set<SeguradoraModel>();
    public DbSet<AgenciadorModel> Agenciadores => Set<AgenciadorModel>();
    public DbSet<GrupoModel> Grupos => Set<GrupoModel>();
    public DbSet<SubgrupoModel> Subgrupos => Set<SubgrupoModel>();
    public DbSet<LotacaoModel> Lotacoes => Set<LotacaoModel>();
    public DbSet<BancoModel> Bancos => Set<BancoModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ClienteConfiguration());
        InfrastructureModelsConfiguration.ApplyConfigurations(modelBuilder);
        
        base.OnModelCreating(modelBuilder);
    }

    public virtual Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Database.BeginTransactionAsync(cancellationToken);
    }
}
