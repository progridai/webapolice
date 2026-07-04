using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using WebApolice.Auditoria.Domain;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Clientes.Domain;
using WebApolice.Modulos.Clientes.Infrastructure.Persistence;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Clientes;

public class AtomicidadeTests : IAsyncLifetime
{
#pragma warning disable CS0618
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:18.4")
        .Build();
#pragma warning restore CS0618

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var connString = _postgreSqlContainer.GetConnectionString();

        using var connection = new NpgsqlConnection(connString);
        await connection.OpenAsync();

        var auditoriaOptions = new DbContextOptionsBuilder<AuditoriaDbContext>()
            .UseNpgsql(connection, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "auditoria"))
            .UseSnakeCaseNamingConvention()
            .Options;
        await using var auditoriaCtx = new AuditoriaDbContext(auditoriaOptions);
        await auditoriaCtx.Database.MigrateAsync();

        var clientesOptions = new DbContextOptionsBuilder<ClientesDbContext>()
            .UseNpgsql(connection, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "clientes"))
            .UseSnakeCaseNamingConvention()
            .Options;
        await using var clientesCtx = new ClientesDbContext(clientesOptions);
        await clientesCtx.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    private (ClientesDbContext, AuditoriaDbContext, DbConnection) CreateContexts(DbConnection connection)
    {
        var auditoriaOptions = new DbContextOptionsBuilder<AuditoriaDbContext>()
            .UseNpgsql(connection, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "auditoria"))
            .UseSnakeCaseNamingConvention()
            .Options;

        var clientesOptions = new DbContextOptionsBuilder<ClientesDbContext>()
            .UseNpgsql(connection, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "clientes"))
            .UseSnakeCaseNamingConvention()
            .Options;

        return (new ClientesDbContext(clientesOptions), new AuditoriaDbContext(auditoriaOptions), connection);
    }

    [Fact]
    public async Task Transacao_BemSucedida_DeveSalvarAmbos()
    {
        var uid = Guid.NewGuid().ToString("N").Substring(0, 30);
        var cpf = "01821765419";

        await using var connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        await connection.OpenAsync();
        var (clientesCtx, auditoriaCtx, conn) = CreateContexts(connection);

        var manager = new ClientesTransactionManager(clientesCtx, auditoriaCtx, conn);

        await manager.ExecuteInTransactionAsync(async () =>
        {
            var cliente = new Cliente(uid, cpf, null, null, null, null);
            clientesCtx.Clientes.Add(cliente);
            await clientesCtx.SaveChangesAsync();

            auditoriaCtx.RegistrosAuditoria.Add(new RegistroAuditoria { Modulo = "Clientes", Acao = "Cadastro", UsuarioIdExterno = "1", RecursoId = uid });
            await auditoriaCtx.SaveChangesAsync();
        });

        // Verificação fora da transação
        await using var verifyConn = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        await verifyConn.OpenAsync();
        var (verifyClientesCtx, verifyAuditoriaCtx, _) = CreateContexts(verifyConn);

        var clienteSalvo = await verifyClientesCtx.Clientes.FirstOrDefaultAsync(c => c.Nome == uid);
        clienteSalvo.Should().NotBeNull();

        var auditoriaSalva = await verifyAuditoriaCtx.RegistrosAuditoria.FirstOrDefaultAsync(a => a.RecursoId == uid);
        auditoriaSalva.Should().NotBeNull();
    }

    [Fact]
    public async Task Transacao_DeveFazerRollbackEmAmbos_QuandoAuditoriaFalhar()
    {
        var uid = Guid.NewGuid().ToString("N").Substring(0, 30);
        var erroNaAuditoria = false;

        await using var connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        await connection.OpenAsync();
        var (clientesCtx, auditoriaCtx, conn) = CreateContexts(connection);

        var manager = new ClientesTransactionManager(clientesCtx, auditoriaCtx, conn);

        try
        {
            await manager.ExecuteInTransactionAsync(async () =>
            {
                var cliente = new Cliente(uid, "93399034393", null, null, null, null);
                clientesCtx.Clientes.Add(cliente);
                await clientesCtx.SaveChangesAsync();

                throw new Exception("Falha simulada na auditoria");
            });
        }
        catch (Exception)
        {
            erroNaAuditoria = true;
        }

        erroNaAuditoria.Should().BeTrue();

        // Verificação fora da transação
        await using var verifyConn = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        await verifyConn.OpenAsync();
        var (verifyClientesCtx, _, _) = CreateContexts(verifyConn);

        var clienteSalvo = await verifyClientesCtx.Clientes.FirstOrDefaultAsync(c => c.Nome == uid);
        clienteSalvo.Should().BeNull("o rollback da transação compartilhada deve desfazer a gravação do cliente.");
    }

    [Fact]
    public async Task Transacao_DeveFazerRollbackEmAmbos_QuandoClienteFalhar()
    {
        var uid = Guid.NewGuid().ToString("N").Substring(0, 30);
        var erroNoCliente = false;

        await using var connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        await connection.OpenAsync();
        var (clientesCtx, auditoriaCtx, conn) = CreateContexts(connection);

        var manager = new ClientesTransactionManager(clientesCtx, auditoriaCtx, conn);

        try
        {
            await manager.ExecuteInTransactionAsync(async () =>
            {
                var cliente = new Cliente(uid, "18692030031", null, null, null, null);
                clientesCtx.Clientes.Add(cliente);
                await clientesCtx.SaveChangesAsync();

                // Simular inserção de cliente com o mesmo CPF (violando unique key)
                var cliente2 = new Cliente(uid + "2", "18692030031", null, null, null, null);
                clientesCtx.Clientes.Add(cliente2);
                await clientesCtx.SaveChangesAsync(); // Deve falhar Unique Constraint
            });
        }
        catch (Exception)
        {
            erroNoCliente = true;
        }

        erroNoCliente.Should().BeTrue();

        // Verificação fora da transação
        await using var verifyConn = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        await verifyConn.OpenAsync();
        var (_, verifyAuditoriaCtx, _) = CreateContexts(verifyConn);

        var auditoriaSalva = await verifyAuditoriaCtx.RegistrosAuditoria.FirstOrDefaultAsync(a => a.RecursoId == uid);
        auditoriaSalva.Should().BeNull("a falha no cliente desfaz tudo");
    }

    [Fact]
    public async Task Transacao_Alteracao_DevePreservarValoresAnterioresQuandoAuditoriaFalha()
    {
        var uid = Guid.NewGuid().ToString("N").Substring(0, 30);

        await using var setupConnection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        await setupConnection.OpenAsync();
        var (setupClientesCtx, setupAuditoriaCtx, setupConn) = CreateContexts(setupConnection);

        var setupManager = new ClientesTransactionManager(setupClientesCtx, setupAuditoriaCtx, setupConn);

        // Insere inicialmente
        await setupManager.ExecuteInTransactionAsync(async () =>
        {
            var cliente = new Cliente(uid, "85115958562", null, null, null, null);
            setupClientesCtx.Clientes.Add(cliente);
            await setupClientesCtx.SaveChangesAsync();
        });

        // Tenta alterar e falha na auditoria
        await using var connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        await connection.OpenAsync();
        var (clientesCtx, auditoriaCtx, conn) = CreateContexts(connection);
        var manager = new ClientesTransactionManager(clientesCtx, auditoriaCtx, conn);

        try
        {
            await manager.ExecuteInTransactionAsync(async () =>
            {
                var cliente = await clientesCtx.Clientes.FirstOrDefaultAsync(c => c.Cpf == "85115958562");
                cliente!.Alterar("Novo Nome", null, null, null);
                await clientesCtx.SaveChangesAsync();

                throw new Exception("Falha na auditoria ao alterar");
            });
        }
        catch
        {
            // esperado
        }

        // Verifica que manteve antigo
        await using var verifyConn = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        await verifyConn.OpenAsync();
        var (verifyClientesCtx, _, _) = CreateContexts(verifyConn);

        var clienteSalvo = await verifyClientesCtx.Clientes.FirstOrDefaultAsync(c => c.Cpf == "85115958562");
        clienteSalvo!.Nome.Should().Be(uid); // manteve original
    }

    [Fact]
    public async Task Transacao_AtivacaoInativacao_NaoMudaStatusQuandoAuditoriaFalha()
    {
        var uid = Guid.NewGuid().ToString("N").Substring(0, 30);

        await using var setupConnection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        await setupConnection.OpenAsync();
        var (setupClientesCtx, setupAuditoriaCtx, setupConn) = CreateContexts(setupConnection);
        var setupManager = new ClientesTransactionManager(setupClientesCtx, setupAuditoriaCtx, setupConn);

        // Insere inicialmente (por padrão Ativo e deixa inativo)
        await setupManager.ExecuteInTransactionAsync(async () =>
        {
            var cliente = new Cliente(uid, "25070302752", null, null, null, null);
            cliente.Inativar(); // Deixa inativo para testar ativar
            setupClientesCtx.Clientes.Add(cliente);
            await setupClientesCtx.SaveChangesAsync();
        });

        // Tenta ativar e falha na auditoria
        await using var connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        await connection.OpenAsync();
        var (clientesCtx, auditoriaCtx, conn) = CreateContexts(connection);
        var manager = new ClientesTransactionManager(clientesCtx, auditoriaCtx, conn);

        try
        {
            await manager.ExecuteInTransactionAsync(async () =>
            {
                var cliente = await clientesCtx.Clientes.FirstOrDefaultAsync(c => c.Cpf == "25070302752");
                cliente!.Ativar();
                await clientesCtx.SaveChangesAsync();

                throw new Exception("Falha na auditoria ao ativar");
            });
        }
        catch
        {
            // esperado
        }

        // Verifica que continua inativo
        await using var verifyConn = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        await verifyConn.OpenAsync();
        var (verifyClientesCtx, _, _) = CreateContexts(verifyConn);

        var clienteSalvo = await verifyClientesCtx.Clientes.FirstOrDefaultAsync(c => c.Cpf == "25070302752");
        clienteSalvo!.Status.Should().Be(StatusCliente.Inativo);
    }
}
