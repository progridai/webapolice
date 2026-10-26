using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace WebApolice.Shared.Infrastructure.Persistence.Design;

public class CoreDbContextFactory : IDesignTimeDbContextFactory<CoreDbContext>
{
    public CoreDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSql");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Defina ConnectionStrings__PostgreSql para executar ferramentas EF.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<CoreDbContext>();

        optionsBuilder
            .UseNpgsql(connectionString, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "core"))
            .UseSnakeCaseNamingConvention();

        return new CoreDbContext(optionsBuilder.Options);
    }
}
