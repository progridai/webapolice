using System.Threading;
using System.Threading.Tasks;

namespace WebApolice.Modulos.Seguranca.Application.Ports;

public interface IProvisionamentoUsuarioService
{
    Task ProvisionarAsync(CancellationToken cancellationToken);
}
