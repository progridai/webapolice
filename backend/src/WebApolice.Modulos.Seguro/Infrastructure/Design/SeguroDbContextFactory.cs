using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
namespace WebApolice.Modulos.Seguro.Infrastructure.Design;
public sealed class SeguroDbContextFactory:IDesignTimeDbContextFactory<SeguroDbContext>
{
    public SeguroDbContext CreateDbContext(string[] args)
    {
        var connection=Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSql")??throw new InvalidOperationException("Configure ConnectionStrings__PostgreSql antes de executar ferramentas EF.");
        return new(new DbContextOptionsBuilder<SeguroDbContext>().UseNpgsql(connection,o=>o.MigrationsHistoryTable("__EFMigrationsHistory","seguro")).UseSnakeCaseNamingConvention().Options);
    }
}
