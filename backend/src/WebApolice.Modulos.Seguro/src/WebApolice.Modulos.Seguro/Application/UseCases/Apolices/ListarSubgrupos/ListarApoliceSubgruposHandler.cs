using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Application.Ports;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubgrupos;

public class ListarApoliceSubgruposHandler
{
    private readonly IApolicesQueries _queries;

    public ListarApoliceSubgruposHandler(IApolicesQueries queries)
    {
        _queries = queries;
    }

    public async Task<List<ApoliceSubgrupoResult>> Handle(
        ListarApoliceSubgruposQuery request,
        CancellationToken cancellationToken)
    {
        return await _queries.ListarSubgruposAsync(request.ApolicePublicId, cancellationToken);
    }
}
