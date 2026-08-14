using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ListarClientes;

public sealed class ListarClientesHandler
{
    private readonly IClientesQueries _queries;

    public ListarClientesHandler(IClientesQueries queries)
    {
        _queries = queries;
    }

    public async Task<ListagemPaginadaResult<ClienteListagemItemResult>> Handle(ListarClientesQuery query, CancellationToken cancellationToken)
    {
        var pagina = query.Pagina > 0 ? query.Pagina : 1;
        var tamanhoPagina = query.TamanhoPagina > 0 ? query.TamanhoPagina : 20;
        if (tamanhoPagina > 100) tamanhoPagina = 100;

        var (itens, totalItens, totalPaginas) = await _queries.ListarPaginadoAsync(
            pagina,
            tamanhoPagina,
            query.Nome,
            query.Documento,
            query.StatusId,
            query.OrdenarPor,
            query.Direcao,
            cancellationToken);

        return new ListagemPaginadaResult<ClienteListagemItemResult>(
            itens,
            pagina,
            tamanhoPagina,
            totalItens,
            totalPaginas
        );
    }
}
