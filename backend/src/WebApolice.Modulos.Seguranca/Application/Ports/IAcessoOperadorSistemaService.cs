using System.Threading;
using System.Threading.Tasks;

namespace WebApolice.Modulos.Seguranca.Application.Ports;

public interface IAcessoOperadorSistemaService
{
    Task<bool> EhOperadorSistemaAsync(string keycloakSub, CancellationToken cancellationToken = default);
}
