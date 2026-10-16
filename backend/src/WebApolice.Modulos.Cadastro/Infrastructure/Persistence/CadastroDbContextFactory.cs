using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence;

public sealed class CadastroDbContextFactory : IDesignTimeDbContextFactory<CadastroDbContext>
{
    public CadastroDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSql");
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("Defina ConnectionStrings__PostgreSql para executar ferramentas EF do Cadastro.");
        return new CadastroDbContext(new DbContextOptionsBuilder<CadastroDbContext>()
            .UseNpgsql(connection, options => options.MigrationsHistoryTable("__EFMigrationsHistory", "cadastro"))
            .UseSnakeCaseNamingConvention().Options);
    }
}
