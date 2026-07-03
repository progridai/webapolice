using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using WebApolice.Shared.Infrastructure.Persistence;
using Xunit;

namespace WebApolice.Integration.Tests.Persistence;

public sealed class InfraestruturaDbContextTests : IAsyncLifetime
{
#pragma warning disable CS0618 // O construtor obsoleto do PostgreSqlBuilder
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:18.4")
        .Build();
#pragma warning restore CS0618

    private InfraestruturaDbContext _dbContext = default!;

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var options = new DbContextOptionsBuilder<InfraestruturaDbContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString(), o => o.MigrationsHistoryTable("__EFMigrationsHistory", "infraestrutura"))
            .UseSnakeCaseNamingConvention()
            .Options;

        _dbContext = new InfraestruturaDbContext(options);
        
        // Aplica a migration num banco recém-criado descartável
        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }

    [Fact]
    public async Task Deve_Conectar_E_Aplicar_Migrations_Sem_Erro()
    {
        // Assert: Se não lançou exceção no InitializeAsync, significa que conectou e aplicou
        var canConnect = await _dbContext.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task Deve_Criar_Schema_Esperado_E_Historico_Migrations()
    {
        // Act
        var tables = await _dbContext.Database.SqlQuery<string>(
            $"SELECT table_name FROM information_schema.tables WHERE table_schema = 'infraestrutura'")
            .ToListAsync();

        // Assert
        tables.Should().Contain("__EFMigrationsHistory");
    }

}
