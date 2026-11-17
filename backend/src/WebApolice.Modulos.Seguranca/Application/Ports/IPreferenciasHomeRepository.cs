using WebApolice.Modulos.Seguranca.Domain;

namespace WebApolice.Modulos.Seguranca.Application.Ports;

public interface IPreferenciasHomeRepository
{
    Task<Usuario?> ObterUsuarioAtivoAsync(string keycloakSub, CancellationToken cancellationToken);
    Task SalvarAsync(CancellationToken cancellationToken);
}
