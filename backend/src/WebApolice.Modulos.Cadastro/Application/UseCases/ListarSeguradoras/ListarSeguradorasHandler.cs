using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ListarSeguradoras;

public sealed class ListarSeguradorasHandler
{
    private readonly ISeguradorasQueries _queries;

    public ListarSeguradorasHandler(ISeguradorasQueries queries)
    {
        _queries = queries;
    }

    public async Task<ListagemPaginadaResult<SeguradoraListagemItemResult>> Handle(ListarSeguradorasQuery query, CancellationToken cancellationToken)
    {
        var pagina = query.Pagina > 0 ? query.Pagina : 1;
        var tamanhoPagina = query.TamanhoPagina > 0 ? query.TamanhoPagina : 20;
        if (tamanhoPagina > 100) tamanhoPagina = 100;

        var (itens, totalItens, totalPaginas) = await _queries.ListarPaginadoAsync(
            pagina,
            tamanhoPagina,
            query.Busca,
            query.Ativo,
            cancellationToken);

        return new ListagemPaginadaResult<SeguradoraListagemItemResult>(
            itens,
            pagina,
            tamanhoPagina,
            totalItens,
            totalPaginas
        );
    }
}
