namespace WebApolice.Auditoria.Contracts;

public interface IContextoAuditoria
{
    string? ObterUsuarioIdExterno();
    string? ObterUsuarioNome();
    string? ObterTraceId();
    string? ObterCorrelationId();
    string? ObterEnderecoIp();
    string? ObterOrigem();
}
