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

    private WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence.FinanceiroDbContext _financeiro = default!;
    private WebApolice.Auditoria.Infrastructure.AuditoriaDbContext _auditoria = default!;
    private Npgsql.NpgsqlConnection _connection = default!;
    public Guid ConvenioPublicId { get; } = Guid.NewGuid();
    public WebApolice.Modulos.Seguro.Application.Ports.ISubgruposCadastro SubgruposCadastro { get; private set; } = default!;
    public SeguroDbContext DbContext { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var connectionString = _postgreSqlContainer.GetConnectionString();

        _connection = new Npgsql.NpgsqlConnection(connectionString);
        await _connection.OpenAsync();
        await using(var cmd = new Npgsql.NpgsqlCommand("CREATE SCHEMA IF NOT EXISTS core; CREATE TABLE core.banco(id bigint PRIMARY KEY,codigo varchar(3),nome text);",_connection)) await cmd.ExecuteNonQueryAsync();
        var financeiro = new WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence.FinanceiroDbContext(new DbContextOptionsBuilder<WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence.FinanceiroDbContext>().UseNpgsql(_connection,o=>o.MigrationsHistoryTable("__EFMigrationsHistory","financeiro")).UseSnakeCaseNamingConvention().Options);
        _financeiro = financeiro;
        await financeiro.Database.MigrateAsync();
        financeiro.ConvenioCobrancas.Add(new(){PublicId=ConvenioPublicId,Nome="Convênio Teste",CreatedAt=DateTime.UtcNow,UpdatedAt=DateTime.UtcNow});
        await financeiro.SaveChangesAsync();
        var auditDb=new WebApolice.Auditoria.Infrastructure.AuditoriaDbContext(new DbContextOptionsBuilder<WebApolice.Auditoria.Infrastructure.AuditoriaDbContext>().UseNpgsql(_connection,o=>o.MigrationsHistoryTable("__EFMigrationsHistory","auditoria")).UseSnakeCaseNamingConvention().Options);
        _auditoria = auditDb;
        await auditDb.Database.MigrateAsync();
        var operacao=new WebApolice.Auditoria.Infrastructure.OperacaoAuditada(auditDb,new WebApolice.Auditoria.Infrastructure.RegistradorAuditoria(auditDb),new Moq.Mock<WebApolice.Auditoria.Contracts.IContextoAuditoria>().Object);
        var convenios=new WebApolice.Modulos.Financeiro.Infrastructure.ConveniosCobrancaRepository(financeiro,operacao);
        var options = new DbContextOptionsBuilder<SeguroDbContext>()
            .UseNpgsql(_connection,o=>o.MigrationsHistoryTable("__EFMigrationsHistory","seguro"))
            .UseSnakeCaseNamingConvention()
            .Options;

        DbContext = new SeguroDbContext(options);
        
        // Aplica todas as Migrations do projeto Seguro automaticamente no container Postgres
        await DbContext.Database.MigrateAsync();
        SubgruposCadastro=new WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories.SubgruposCadastroRepository(DbContext,convenios,operacao);
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _financeiro.DisposeAsync();
        await _auditoria.DisposeAsync();
        await _connection.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }
}
