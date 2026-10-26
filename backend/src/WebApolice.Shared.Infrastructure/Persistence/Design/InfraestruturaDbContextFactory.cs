using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebApolice.Shared.Infrastructure.Persistence.Design;

public class InfraestruturaDbContextFactory : IDesignTimeDbContextFactory<InfraestruturaDbContext>
{
    public InfraestruturaDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSql");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Defina ConnectionStrings__PostgreSql antes de executar ferramentas EF de infraestrutura.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<InfraestruturaDbContext>();

        optionsBuilder
            .UseNpgsql(connectionString, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "infraestrutura"))
            .UseSnakeCaseNamingConvention();

        return new InfraestruturaDbContext(optionsBuilder.Options);
    }
}
