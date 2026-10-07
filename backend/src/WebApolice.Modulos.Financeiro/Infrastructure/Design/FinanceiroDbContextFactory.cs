using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence;
namespace WebApolice.Modulos.Financeiro.Infrastructure.Design;
public sealed class FinanceiroDbContextFactory:IDesignTimeDbContextFactory<FinanceiroDbContext>
{
    public FinanceiroDbContext CreateDbContext(string[] args)
    {
        var connection=Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSql")??throw new InvalidOperationException("Configure ConnectionStrings__PostgreSql antes de executar ferramentas EF.");
        return new(new DbContextOptionsBuilder<FinanceiroDbContext>().UseNpgsql(connection,o=>o.MigrationsHistoryTable("__EFMigrationsHistory","financeiro")).UseSnakeCaseNamingConvention().Options);
    }
}
