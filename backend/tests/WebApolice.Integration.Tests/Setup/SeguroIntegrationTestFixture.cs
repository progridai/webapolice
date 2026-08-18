using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using Xunit;

namespace WebApolice.Integration.Tests.Setup;

public class SeguroIntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .Build();

    public SeguroDbContext DbContext { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var connectionString = _postgreSqlContainer.GetConnectionString();

        var options = new DbContextOptionsBuilder<SeguroDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        DbContext = new SeguroDbContext(options);
        
        // Aplica todas as Migrations do projeto Seguro automaticamente no container Postgres
        await DbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }
}
