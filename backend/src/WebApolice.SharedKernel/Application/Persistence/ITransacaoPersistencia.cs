using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApolice.SharedKernel.Application.Persistence;

/// <summary>Controle da transação local sem expor tipos de um provider de persistência.</summary>
public interface ITransacaoPersistencia : IDisposable, IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
