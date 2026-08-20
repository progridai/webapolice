using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.SharedKernel.Application.Models;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarApolices;

public sealed class ListarApolicesHandler
{
    private readonly IApolicesQueries _queries;

    public ListarApolicesHandler(IApolicesQueries queries)
    {
        _queries = queries;
    }

    public async Task<PagedResult<ApoliceListagemItemResult>> Handle(ListarApolicesQuery query, CancellationToken cancellationToken)
    {
        var pagina = query.Pagina > 0 ? query.Pagina : 1;
        var tamanhoPagina = query.TamanhoPagina > 0 ? query.TamanhoPagina : 20;
        if (tamanhoPagina > 100) tamanhoPagina = 100;

        return await _queries.ListarPaginadoAsync(
            pagina,
            tamanhoPagina,
            query.Busca,
            query.Status,
            query.Ativo,
            query.EstipulanteId,
            query.SeguradoraId,
            query.TipoRamo,
            query.VigenciaDataReferencia,
            cancellationToken);
    }
}
