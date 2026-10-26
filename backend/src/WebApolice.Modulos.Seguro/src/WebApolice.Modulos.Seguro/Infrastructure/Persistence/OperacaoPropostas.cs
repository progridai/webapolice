using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence;

public sealed class OperacaoPropostas(SeguroDbContext db, OperacaoAuditada operacao) : IOperacaoPropostas
{
    public Task<T> ExecutarAsync<T>(string acao,
        Func<CancellationToken, Task<(T Resultado, string PublicId, object? Antes, object? Depois)>> executar,
        CancellationToken cancellationToken) =>
        operacao.ExecutarAsync(db, "Seguro", "proposta", acao, executar, cancellationToken);
}
