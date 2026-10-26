namespace WebApolice.Modulos.Seguro.Application.Ports;

public interface IOperacaoPropostas
{
    Task<T> ExecutarAsync<T>(string acao,
        Func<CancellationToken, Task<(T Resultado, string PublicId, object? Antes, object? Depois)>> executar,
        CancellationToken cancellationToken);
}
