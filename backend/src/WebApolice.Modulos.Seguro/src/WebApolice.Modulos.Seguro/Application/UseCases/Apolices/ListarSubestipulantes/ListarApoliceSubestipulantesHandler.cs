using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Application.Ports;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubestipulantes;

public class ListarApoliceSubestipulantesHandler
{
    private readonly IApolicesQueries _queries;

    public ListarApoliceSubestipulantesHandler(IApolicesQueries queries)
    {
        _queries = queries;
    }

    public async Task<List<ApoliceSubestipulanteResult>> Handle(ListarApoliceSubestipulantesQuery request, CancellationToken cancellationToken)
    {
        return await _queries.ListarSubestipulantesAsync(request.ApolicePublicId, cancellationToken);
    }
}
