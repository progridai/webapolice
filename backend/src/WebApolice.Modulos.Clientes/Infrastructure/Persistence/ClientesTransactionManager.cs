using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Clientes.Application.Ports;

namespace WebApolice.Modulos.Clientes.Infrastructure.Persistence;

public sealed class ClientesTransactionManager : IClientesTransactionManager
{
    private readonly ClientesDbContext _clientesDbContext;
    private readonly AuditoriaDbContext _auditoriaDbContext;
    private readonly DbConnection _sharedConnection;

    public ClientesTransactionManager(
        ClientesDbContext clientesDbContext,
        AuditoriaDbContext auditoriaDbContext,
        DbConnection sharedConnection)
    {
        _clientesDbContext = clientesDbContext;
        _auditoriaDbContext = auditoriaDbContext;
        _sharedConnection = sharedConnection;
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        if (_sharedConnection.State != System.Data.ConnectionState.Open)
        {
            await _sharedConnection.OpenAsync(cancellationToken);
        }

        await using var transaction = await _sharedConnection.BeginTransactionAsync(cancellationToken);

        await _clientesDbContext.Database.UseTransactionAsync(transaction, cancellationToken);
        await _auditoriaDbContext.Database.UseTransactionAsync(transaction, cancellationToken);

        try
        {
            await action();
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _clientesDbContext.Database.UseTransactionAsync(null, cancellationToken);
            await _auditoriaDbContext.Database.UseTransactionAsync(null, cancellationToken);
        }
    }
}
