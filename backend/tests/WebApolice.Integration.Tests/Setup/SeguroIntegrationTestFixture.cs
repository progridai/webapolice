using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Testcontainers.PostgreSql;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.Modulos.Financeiro.Contracts;
using WebApolice.Modulos.Financeiro.Infrastructure;
using WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

namespace WebApolice.Integration.Tests.Setup;

/// <summary>Base vazia própria: modelo Cadastro atual e migrations de Financeiro/Auditoria/Seguro.</summary>
public class SeguroIntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.4").Build();
    private readonly string? _migrationAlvo;
    public SeguroIntegrationTestFixture() { }
    internal SeguroIntegrationTestFixture(string migrationAlvo) => _migrationAlvo = migrationAlvo;

    internal string ConnectionString => _container.GetConnectionString();
    public NpgsqlConnection Connection { get; private set; } = default!;
    public CadastroDbContext Cadastro { get; private set; } = default!;
    public FinanceiroDbContext Financeiro { get; private set; } = default!;
    public AuditoriaDbContext Auditoria { get; private set; } = default!;
    public SeguroDbContext DbContext { get; private set; } = default!;
    public Guid ConvenioPublicId { get; } = Guid.NewGuid();
    public IConveniosCobrancaConsulta ConveniosConsulta { get; private set; } = default!;
    public ISubgruposCadastro SubgruposCadastro { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        Connection = new NpgsqlConnection(_container.GetConnectionString());
        await Connection.OpenAsync();
        Cadastro = new CadastroEfemeroDbContext(new DbContextOptionsBuilder<CadastroDbContext>()
            .UseNpgsql(Connection).UseSnakeCaseNamingConvention().Options);
        await Cadastro.Database.EnsureCreatedAsync();
        await Cadastro.Database.ExecuteSqlRawAsync("INSERT INTO cadastro.cliente_status(id,codigo,nome,ativo) VALUES (1,'ativo','Ativo',true),(2,'inativo','Inativo',true)");
        Financeiro = new FinanceiroDbContext(new DbContextOptionsBuilder<FinanceiroDbContext>()
            .UseNpgsql(Connection, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "financeiro")).UseSnakeCaseNamingConvention().Options);
        await Financeiro.Database.MigrateAsync();
        Financeiro.ConvenioCobrancas.Add(new() { PublicId = ConvenioPublicId, Nome = "Convênio Teste", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await Financeiro.SaveChangesAsync();
        Auditoria = new AuditoriaDbContext(new DbContextOptionsBuilder<AuditoriaDbContext>()
            .UseNpgsql(Connection, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "auditoria")).UseSnakeCaseNamingConvention().Options);
        await Auditoria.Database.MigrateAsync();
        var operacao = new OperacaoAuditada(Auditoria, new RegistradorAuditoria(Auditoria), new Moq.Mock<IContextoAuditoria>().Object);
        var convenios = new ConveniosCobrancaRepository(Financeiro, operacao);
        ConveniosConsulta = convenios;
        DbContext = new SeguroDbContext(new DbContextOptionsBuilder<SeguroDbContext>()
            .UseNpgsql(Connection, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "seguro")).UseSnakeCaseNamingConvention().Options);
        await DbContext.GetService<IMigrator>().MigrateAsync(_migrationAlvo);
        SubgruposCadastro = new SubgruposCadastroRepository(DbContext, convenios, operacao);
    }

    public async Task DisposeAsync()
    {
        if (DbContext is not null) await DbContext.DisposeAsync();
        if (Auditoria is not null) await Auditoria.DisposeAsync();
        if (Financeiro is not null) await Financeiro.DisposeAsync();
        if (Cadastro is not null) await Cadastro.DisposeAsync();
        if (Connection is not null) await Connection.DisposeAsync();
        await _container.DisposeAsync();
    }
}
