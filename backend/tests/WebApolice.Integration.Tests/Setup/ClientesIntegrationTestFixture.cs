using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using Xunit;

namespace WebApolice.Integration.Tests.Setup;

public class ClientesIntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .Build();

    public CadastroDbContext DbContext { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var connectionString = _postgreSqlContainer.GetConnectionString();

        // 1. Conectar e rodar o script canÃ´nico
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var sqlPath = Path.Combine("Setup", "schema_clientes.sql");
        var sqlScript = await File.ReadAllTextAsync(sqlPath);
        
        await using var command = new NpgsqlCommand(sqlScript, connection);
        await command.ExecuteNonQueryAsync();

        // 2. Configurar o DbContext
        var options = new DbContextOptionsBuilder<CadastroDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        DbContext = new CadastroDbContext(options);
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }
}
