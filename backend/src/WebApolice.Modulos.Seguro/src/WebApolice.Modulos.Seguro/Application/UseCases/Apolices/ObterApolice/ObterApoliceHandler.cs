using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Application.Ports;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterApolice;

public sealed class ObterApoliceHandler
{
    private readonly IApolicesQueries _queries;

    public ObterApoliceHandler(IApolicesQueries queries)
    {
        _queries = queries;
    }

    public async Task<ApoliceDetalheResult?> Handle(ObterApolicePorPublicIdQuery query, CancellationToken cancellationToken)
    {
        return await _queries.ObterDetalhePorPublicIdAsync(query.PublicId, cancellationToken);
    }
}
