using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguranca.Domain;
using WebApolice.Modulos.Seguranca.Domain.Auditoria;
using WebApolice.Modulos.Seguranca.Domain.Relacionamentos;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

public class SegurancaDbContext : DbContext
{
    public SegurancaDbContext(DbContextOptions<SegurancaDbContext> options) : base(options)
    {
    }

    public DbSet<Modulo> Modulos => Set<Modulo>();
    public DbSet<Recurso> Recursos => Set<Recurso>();
    public DbSet<Permissao> Permissoes => Set<Permissao>();
    public DbSet<Perfil> Perfis => Set<Perfil>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<PerfilPermissao> PerfisPermissoes => Set<PerfilPermissao>();
    public DbSet<UsuarioPerfil> UsuariosPerfis => Set<UsuarioPerfil>();
    public DbSet<AuditoriaPermissao> AuditoriaPermissoes => Set<AuditoriaPermissao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SegurancaDbContext).Assembly);
    }
}
