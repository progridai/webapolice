using System.Threading;
using System.Threading.Tasks;

namespace WebApolice.SharedKernel.Application.Ports;

public interface ILocalidadeResolver
{
    Task<(long? CidadeId, long? EstadoId)> ResolverLocalidadeAsync(string cidade, string uf, CancellationToken cancellationToken);
}
