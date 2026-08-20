using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.SharedKernel.Application.Models;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarHistorico;

public class ListarApoliceHistoricoHandler
{
    private readonly IApolicesQueries _queries;

    public ListarApoliceHistoricoHandler(IApolicesQueries queries)
    {
        _queries = queries;
    }

    public async Task<PagedResult<ApoliceHistoricoResult>> Handle(ListarApoliceHistoricoQuery request, CancellationToken cancellationToken)
    {
        return await _queries.ListarHistoricoPaginadoAsync(request.ApolicePublicId, request.Pagina, request.TamanhoPagina, cancellationToken);
    }
}
