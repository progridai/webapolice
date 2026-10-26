using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebApolice.Auditoria.Infrastructure;

public class AuditoriaDbContextFactory : IDesignTimeDbContextFactory<AuditoriaDbContext>
{
    public AuditoriaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuditoriaDbContext>();
        
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSql");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Defina ConnectionStrings__PostgreSql para executar ferramentas EF.");

        optionsBuilder.UseNpgsql(connectionString, 
            o => 
            {
                o.MigrationsHistoryTable("__EFMigrationsHistory", "auditoria");
            })
            .UseSnakeCaseNamingConvention();

        return new AuditoriaDbContext(optionsBuilder.Options);
    }
}
