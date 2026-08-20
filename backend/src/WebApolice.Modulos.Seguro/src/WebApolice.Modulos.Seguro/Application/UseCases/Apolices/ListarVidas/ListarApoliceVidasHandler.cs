using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.SharedKernel.Application.Models;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas;

public sealed class ListarApoliceVidasHandler
{
    private readonly IApolicesQueries _queries;

    public ListarApoliceVidasHandler(IApolicesQueries queries)
    {
        _queries = queries;
    }

    public async Task<PagedResult<ApoliceVidaResult>> Handle(ListarApoliceVidasQuery query, CancellationToken cancellationToken)
    {
        var pagina = query.Pagina > 0 ? query.Pagina : 1;
        var tamanhoPagina = query.TamanhoPagina > 0 ? query.TamanhoPagina : 20;
        if (tamanhoPagina > 100) tamanhoPagina = 100;

        return await _queries.ListarVidasPaginadoAsync(
            query.ApolicePublicId,
            pagina,
            tamanhoPagina,
            cancellationToken);
    }
}
