namespace WebApolice.Modulos.Seguranca.Application.Ports;

public interface IOperacaoSegurancaAuditada
{
    Task<T> ExecutarAsync<T>(string recurso, string acao,
        Func<CancellationToken, Task<(T Resultado, string PublicId, object? Antes, object? Depois)>> executar,
        CancellationToken cancellationToken);
}
