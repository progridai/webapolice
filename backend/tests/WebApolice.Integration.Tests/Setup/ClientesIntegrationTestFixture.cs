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
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder("postgres:18.4")
        .Build();

    public CadastroDbContext DbContext { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var connectionString = _postgreSqlContainer.GetConnectionString();

        var options = new DbContextOptionsBuilder<CadastroDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        DbContext = new CadastroEfemeroDbContext(options);
        await DbContext.Database.EnsureCreatedAsync();
        await DbContext.Database.ExecuteSqlRawAsync("INSERT INTO cadastro.cliente_status(id,codigo,nome,ativo) VALUES (1,'ativo','Ativo',true),(2,'inativo','Inativo',true)");
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }
}
