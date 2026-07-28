using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguranca.Application.DTOs;

namespace WebApolice.Modulos.Seguranca.Application.Ports;

public interface IPermissoesEfetivasService
{
    Task<PermissoesEfetivasUsuario> CalcularPermissoesAsync(string keycloakSub, CancellationToken cancellationToken = default);
}
