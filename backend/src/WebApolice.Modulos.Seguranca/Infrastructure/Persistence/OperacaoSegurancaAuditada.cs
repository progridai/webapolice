using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Seguranca.Application.Ports;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

public sealed class OperacaoSegurancaAuditada(SegurancaDbContext db, OperacaoAuditada operacao) : IOperacaoSegurancaAuditada
{
    public Task<T> ExecutarAsync<T>(string recurso, string acao,
        Func<CancellationToken, Task<(T Resultado, string PublicId, object? Antes, object? Depois)>> executar,
        CancellationToken cancellationToken) =>
        operacao.ExecutarAsync(db, "Seguranca", recurso, acao, executar, cancellationToken);
}
