using WebApolice.Auditoria.Domain;

namespace WebApolice.Auditoria.Contracts;

public interface IRegistradorAuditoria
{
    Task RegistrarAsync(RegistroAuditoria registro, CancellationToken cancellationToken = default);
}
