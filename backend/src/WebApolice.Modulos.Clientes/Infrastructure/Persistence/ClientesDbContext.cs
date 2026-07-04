using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Clientes.Domain;
using WebApolice.Modulos.Clientes.Infrastructure.Persistence.Configurations;

namespace WebApolice.Modulos.Clientes.Infrastructure.Persistence;

public sealed class ClientesDbContext : DbContext
{
    public ClientesDbContext(DbContextOptions<ClientesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("clientes");
        modelBuilder.ApplyConfiguration(new ClienteConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}
