using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Cadastro.Application.Ports;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence;

public sealed class OperacaoCadastroAuditada(CadastroDbContext db, OperacaoAuditada operacao) : IOperacaoCadastroAuditada
{
    public Task<T> ExecutarAsync<T>(string modulo, string recurso, string acao,
        Func<CancellationToken, Task<(T Resultado, string PublicId, object? Antes, object? Depois)>> executar,
        CancellationToken cancellationToken) =>
        operacao.ExecutarAsync(db, modulo, recurso, acao, executar, cancellationToken);
}
