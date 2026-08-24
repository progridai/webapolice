using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterApoliceVida;

public sealed class ObterApoliceVidaHandler
{
    private readonly IApolicesQueries _queries;

    public ObterApoliceVidaHandler(IApolicesQueries queries)
    {
        _queries = queries;
    }

    public async Task<ApoliceVidaResult?> Handle(ObterApoliceVidaQuery query, CancellationToken cancellationToken)
    {
        return await _queries.ObterApoliceVidaPorPublicIdAsync(
            query.ApolicePublicId,
            query.ApoliceVidaPublicId,
            cancellationToken);
    }
}
