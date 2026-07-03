using Microsoft.EntityFrameworkCore;

namespace WebApolice.Shared.Infrastructure.Persistence;

public sealed class InfraestruturaDbContext : DbContext
{
    public InfraestruturaDbContext(DbContextOptions<InfraestruturaDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("infraestrutura");
    }
}
