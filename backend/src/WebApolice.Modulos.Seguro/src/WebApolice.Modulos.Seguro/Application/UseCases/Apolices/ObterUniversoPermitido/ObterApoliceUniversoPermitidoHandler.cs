using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Application.Ports;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterUniversoPermitido;

public class ObterApoliceUniversoPermitidoHandler
{
    private readonly IApolicesQueries _queries;

    public ObterApoliceUniversoPermitidoHandler(IApolicesQueries queries)
    {
        _queries = queries;
    }

    public async Task<ApoliceUniversoPermitidoResult?> Handle(ObterApoliceUniversoPermitidoQuery request, CancellationToken cancellationToken)
    {
        return await _queries.ObterUniversoPermitidoAsync(request.ApolicePublicId, cancellationToken);
    }
}
