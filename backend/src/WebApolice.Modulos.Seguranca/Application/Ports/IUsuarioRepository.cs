using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguranca.Application.DTOs;

namespace WebApolice.Modulos.Seguranca.Application.Ports;

public interface IUsuarioRepository
{
    Task<DadosUsuarioPermissoes?> ObterDadosPermissoesPorKeycloakSubAsync(string keycloakSub, CancellationToken cancellationToken = default);
}
