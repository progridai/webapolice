using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;
using Xunit;

namespace WebApolice.Integration.Tests.Setup;

/// <summary>
/// Fixture de integração para o módulo de Segurança.
/// Utiliza um container PostgreSQL descartável (Testcontainers) para garantir
/// total isolamento dos dados de desenvolvimento/teste. Nunca toca no banco real.
/// </summary>
public class SegurancaIntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .Build();

    public SegurancaDbContext DbContext { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var connectionString = _postgreSqlContainer.GetConnectionString();

        var options = new DbContextOptionsBuilder<SegurancaDbContext>()
            .UseNpgsql(connectionString, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "seguranca"))
            .UseSnakeCaseNamingConvention()
            .Options;

        DbContext = new SegurancaDbContext(options);

        // Aplica todas as Migrations no container descartável — nunca afeta o banco real
        await DbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (DbContext != null)
        {
            await DbContext.DisposeAsync();
        }
        await _postgreSqlContainer.DisposeAsync();
    }
}
