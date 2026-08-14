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
            connectionString = "Host=painel.bravida.com.br;Port=5432;Database=webapolice_teste;Username=bravito;Password=Bravida@2023!";
        }

        var optionsBuilder = new DbContextOptionsBuilder<CoreDbContext>();

        optionsBuilder
            .UseNpgsql(connectionString, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "core"))
            .UseSnakeCaseNamingConvention();

        return new CoreDbContext(optionsBuilder.Options);
    }
}
