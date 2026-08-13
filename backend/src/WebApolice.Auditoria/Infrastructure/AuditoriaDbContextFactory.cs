using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebApolice.Auditoria.Infrastructure;

public class AuditoriaDbContextFactory : IDesignTimeDbContextFactory<AuditoriaDbContext>
{
    public AuditoriaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuditoriaDbContext>();
        
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSql") 
            ?? "Host=localhost;Port=5432;Database=webapolice;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString, 
            o => 
            {
                o.MigrationsHistoryTable("__EFMigrationsHistory", "auditoria");
            })
            .UseSnakeCaseNamingConvention();

        return new AuditoriaDbContext(optionsBuilder.Options);
    }
}
