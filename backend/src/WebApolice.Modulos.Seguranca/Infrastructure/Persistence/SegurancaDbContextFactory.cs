using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

public class SegurancaDbContextFactory : IDesignTimeDbContextFactory<SegurancaDbContext>
{
    public SegurancaDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "WebApolice.Api");
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var builder = new DbContextOptionsBuilder<SegurancaDbContext>();
        var connectionString = configuration.GetConnectionString("PostgreSql");

        // Utilizando o UseSnakeCaseNamingConvention exigido pelas convenções do projeto
        builder.UseNpgsql(connectionString)
               .UseSnakeCaseNamingConvention();

        return new SegurancaDbContext(builder.Options);
    }
}
