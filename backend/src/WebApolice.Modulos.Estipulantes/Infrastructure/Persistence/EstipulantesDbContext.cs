using System.Reflection;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Estipulantes.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Estipulantes.Infrastructure.Persistence;

public class EstipulantesDbContext(DbContextOptions<EstipulantesDbContext> options) : DbContext(options)
{
    public DbSet<PessoaModel> Pessoas { get; set; }
    public DbSet<PessoaEnderecoModel> Enderecos { get; set; }
    public DbSet<PessoaContatoModel> Contatos { get; set; }
    public DbSet<PessoaContatoInstitucionalModel> ContatosInstitucionais { get; set; }
    public DbSet<EstipulanteModel> Estipulantes { get; set; }
    public DbSet<EstipulanteConfiguracaoModel> Configuracoes { get; set; }
    public DbSet<GrupoModel> Grupos { get; set; }
    public DbSet<SeguradoraModel> Seguradoras { get; set; }
    public DbSet<CidadeModel> Cidades { get; set; }
    public DbSet<ClienteModel> Clientes { get; set; }
    public DbSet<SubestipulanteModel> Subestipulantes { get; set; }
    public DbSet<CorretoraModel> Corretoras { get; set; }
    public DbSet<AgenciadorModel> Agenciadores { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
