using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubgrupos;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterSubgrupo;

public class ObterApoliceSubgrupoHandler
{
    private readonly IApolicesQueries _queries;

    public ObterApoliceSubgrupoHandler(IApolicesQueries queries)
    {
        _queries = queries;
    }

    public async Task<ApoliceSubgrupoResult?> Handle(
        ObterApoliceSubgrupoPorPublicIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _queries.ObterSubgrupoPorPublicIdAsync(
            request.ApolicePublicId,
            request.SubgrupoPublicId,
            cancellationToken);
    }
}
