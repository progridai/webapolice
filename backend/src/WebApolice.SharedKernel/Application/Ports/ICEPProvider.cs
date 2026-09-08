using System.Threading;
using System.Threading.Tasks;
using WebApolice.SharedKernel.Application.Models;

namespace WebApolice.SharedKernel.Application.Ports;

public interface ICEPProvider
{
    Task<EnderecoProviderDto?> ConsultarCEPAsync(string cep, CancellationToken cancellationToken);
}
