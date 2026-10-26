namespace WebApolice.Modulos.Seguranca.Contracts;

/// <summary>Consulta pública de disponibilidade, sem expor tabelas ou modelos internos.</summary>
public interface IDisponibilidadeCatalogo
{
    Task<bool> RecursoDisponivelAsync(string modulo, string recurso, CancellationToken cancellationToken);
    Task<bool> PermissaoDisponivelAsync(string permissao, CancellationToken cancellationToken);
}
