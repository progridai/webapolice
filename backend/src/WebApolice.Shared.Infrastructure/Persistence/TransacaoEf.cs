using Microsoft.EntityFrameworkCore.Storage;
using WebApolice.SharedKernel.Application.Persistence;

namespace WebApolice.Shared.Infrastructure.Persistence;

public sealed class TransacaoEf(IDbContextTransaction transacao) : ITransacaoPersistencia
{
    public Task CommitAsync(CancellationToken cancellationToken = default) => transacao.CommitAsync(cancellationToken);
    public Task RollbackAsync(CancellationToken cancellationToken = default) => transacao.RollbackAsync(cancellationToken);
    public void Dispose() => transacao.Dispose();
    public ValueTask DisposeAsync() => transacao.DisposeAsync();
}
