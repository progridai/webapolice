using WebApolice.Integration.Tests.Setup;
using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;
using Testcontainers.PostgreSql;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Cadastro.Application.UseCases.AlterarCliente;
using WebApolice.Modulos.Cadastro.Domain;
using WebApolice.Modulos.Cadastro.Domain.Clientes;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Repositories;
using WebApolice.Modulos.Seguranca.Contracts;

namespace WebApolice.Integration.Tests.Modulos.Clientes;

// Banco novo por teste; não aceita conexão externa nem usa esquema/dados do legado.
public class ClienteAuditadoPostgresTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18.4").Build();
    public Task InitializeAsync() => _postgres.StartAsync();
    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FalhaDaAuditoriaNaoConfirmaAlteracao(bool cancelamento)
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await using var db = CriarCadastro(connection);
        await PrepararAsync(db);
        var cliente = await CriarClienteAsync(db);
        await using var auditoriaDb = CriarAuditoria(connection);
        var registrador = new Mock<IRegistradorAuditoria>();
        registrador.Setup(r => r.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(cancelamento ? new OperationCanceledException() : new InvalidOperationException("falha-controlada-auditoria"));
        var handler = CriarHandler(db, auditoriaDb, registrador.Object);
        var command = new AlterarClienteCommand(cliente.PublicId, "Nome Alterado", null,
            new DateOnly(1990, 1, 1), null, null, false, null, [], []);
        await Assert.ThrowsAnyAsync<Exception>(() => handler.Handle(command, "sub-teste", default));

        // Nova conexão, sem depender de objetos rastreados pelo contexto da operação.
        await using var verifyConnection = new NpgsqlConnection(_postgres.GetConnectionString());
        await using var verify = CriarCadastro(verifyConnection);
        Assert.Equal("Nome Original", (await verify.Pessoas.SingleAsync()).Nome);
        Assert.Empty(db.ChangeTracker.Entries());
        Assert.Empty(auditoriaDb.ChangeTracker.Entries());
    }

    [Fact]
    public async Task SucessoConfirmaNegocioEAuditoriaNaMesmaOperacao()
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await using var db = CriarCadastro(connection);
        await PrepararAsync(db);
        var cliente = await CriarClienteAsync(db);
        await using var auditoriaDb = CriarAuditoria(connection);
        await auditoriaDb.Database.MigrateAsync();
        var handler = CriarHandler(db, auditoriaDb, new RegistradorAuditoria(auditoriaDb));
        await handler.Handle(new AlterarClienteCommand(cliente.PublicId, "NOME ALTERADO", null,
            new DateOnly(1990, 1, 1), null, null, false, null, [], []), "sub-teste", default);
        db.ChangeTracker.Clear();
        Assert.Equal("Nome Alterado", (await db.Pessoas.SingleAsync()).Nome);
        Assert.Equal("CLIENTE_ALTERADO", (await auditoriaDb.RegistrosAuditoria.SingleAsync()).Acao);
    }

    private static AlterarClienteHandler CriarHandler(CadastroDbContext db, AuditoriaDbContext auditoriaDb, IRegistradorAuditoria registrador) =>
        new(new ClienteRepository(db), new OperacaoCadastroAuditada(db,
            new OperacaoAuditada(auditoriaDb, registrador, Mock.Of<IContextoAuditoria>())), Mock.Of<IDisponibilidadeCatalogo>());

    private static CadastroEfemeroDbContext CriarCadastro(NpgsqlConnection connection) => new(
        new DbContextOptionsBuilder<CadastroDbContext>().UseNpgsql(connection).UseSnakeCaseNamingConvention().Options);

    private static AuditoriaDbContext CriarAuditoria(NpgsqlConnection connection) => new(
        new DbContextOptionsBuilder<AuditoriaDbContext>().UseNpgsql(connection,
            o => o.MigrationsHistoryTable("__EFMigrationsHistory", "auditoria")).UseSnakeCaseNamingConvention().Options);

    private static async Task PrepararAsync(CadastroDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        await db.Database.ExecuteSqlRawAsync("INSERT INTO cadastro.cliente_status (id,codigo,nome,ativo) VALUES (1,'ativo','Ativo',true)");
    }

    private static async Task<Cliente> CriarClienteAsync(CadastroDbContext db)
    {
        var pessoa = new PessoaModel(1, "Nome Original", "52998224725", "52998224725", true,
            new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc), null, null);
        db.Pessoas.Add(pessoa);
        await db.SaveChangesAsync();
        var cliente = new Cliente(pessoa.Id, 1);
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();
        return cliente;
    }

}
