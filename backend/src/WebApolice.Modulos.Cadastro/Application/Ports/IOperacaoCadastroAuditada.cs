namespace WebApolice.Modulos.Cadastro.Application.Ports;

/// <summary>Confirma a alteração e sua auditoria como uma única operação local.</summary>
public interface IOperacaoCadastroAuditada
{
    Task<T> ExecutarAsync<T>(string modulo, string recurso, string acao,
        Func<CancellationToken, Task<(T Resultado, string PublicId, object? Antes, object? Depois)>> executar,
        CancellationToken cancellationToken);
}
