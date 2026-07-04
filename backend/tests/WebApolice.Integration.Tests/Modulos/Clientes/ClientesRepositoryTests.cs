using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using WebApolice.Modulos.Clientes.Domain;
using WebApolice.Modulos.Clientes.Infrastructure.Persistence;
using WebApolice.Modulos.Clientes.Domain.Exceptions;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Clientes;

public class ClientesRepositoryTests : IAsyncLifetime
{
#pragma warning disable CS0618
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:18.4")
        .Build();
#pragma warning restore CS0618

    private ClientesDbContext _dbContext = default!;
    private ClientesRepository _repository = default!;

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var options = new DbContextOptionsBuilder<ClientesDbContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString(), o => o.MigrationsHistoryTable("__EFMigrationsHistory", "clientes"))
            .UseSnakeCaseNamingConvention()
            .Options;

        _dbContext = new ClientesDbContext(options);
        await _dbContext.Database.MigrateAsync();
        
        _repository = new ClientesRepository(_dbContext);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }

    [Fact]
    public async Task Adicionar_ClienteValido_DevePersistirComSucesso()
    {
        var cliente = new Cliente("Teste Silva", "18692030031", null, null, null, null);
        await _repository.AdicionarAsync(cliente, CancellationToken.None);
        await _repository.SalvarAlteracoesAsync(CancellationToken.None);

        var persistido = await _repository.ObterPorIdAsync(cliente.Id, CancellationToken.None);
        persistido.Should().NotBeNull();
        persistido!.Nome.Should().Be("Teste Silva");
    }

    [Fact]
    public async Task Adicionar_CpfDuplicado_DeveLancarConflictException()
    {
        var cliente1 = new Cliente("Teste A", "01821765419", null, null, null, null);
        await _repository.AdicionarAsync(cliente1, CancellationToken.None);
        await _repository.SalvarAlteracoesAsync(CancellationToken.None);

        var cliente2 = new Cliente("Teste B", "01821765419", null, null, null, null);
        await _repository.AdicionarAsync(cliente2, CancellationToken.None);

        var act = () => _repository.SalvarAlteracoesAsync(CancellationToken.None);
        await act.Should().ThrowAsync<ClienteJaCadastradoException>();
    }

    [Fact]
    public async Task DuplicidadeConcorrente_ApenasUmDeveTerSucesso()
    {
        // Limpa estado para este teste ser o primeiro
        _dbContext.ChangeTracker.Clear();
        var numTasks = 5;
        var options = new DbContextOptionsBuilder<ClientesDbContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString(), o => o.MigrationsHistoryTable("__EFMigrationsHistory", "clientes"))
            .UseSnakeCaseNamingConvention()
            .Options;

        var tasks = Enumerable.Range(1, numTasks).Select(async i =>
        {
            await using var context = new ClientesDbContext(options);
            var repo = new ClientesRepository(context);
            var cliente = new Cliente($"Concorrente {i}", "93399034393", null, null, null, null);
            await repo.AdicionarAsync(cliente, CancellationToken.None);
            try
            {
                await repo.SalvarAlteracoesAsync(CancellationToken.None);
                return true;
            }
            catch (ClienteJaCadastradoException)
            {
                return false;
            }
        });

        var results = await Task.WhenAll(tasks);

        results.Count(r => r).Should().Be(1);
        results.Count(r => !r).Should().Be(numTasks - 1);
    }
}
