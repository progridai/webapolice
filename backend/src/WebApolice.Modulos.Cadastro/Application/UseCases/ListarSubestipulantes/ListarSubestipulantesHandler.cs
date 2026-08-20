using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ListarSubestipulantes;

public sealed class ListarSubestipulantesHandler
{
    private readonly ISubestipulantesQueries _queries;

    public ListarSubestipulantesHandler(ISubestipulantesQueries queries)
    {
        _queries = queries;
    }

    public async Task<ListagemPaginadaResult<SubestipulanteListagemItemResult>> Handle(ListarSubestipulantesQuery query, CancellationToken cancellationToken)
    {
        var (itens, totalItens, totalPaginas) = await _queries.ListarPaginadoAsync(
            query.Pagina, 
            query.TamanhoPagina, 
            query.Busca, 
            query.Ativo, 
            cancellationToken);

        return new ListagemPaginadaResult<SubestipulanteListagemItemResult>(
            itens,
            query.Pagina,
            query.TamanhoPagina,
            totalItens,
            totalPaginas
        );
    }
}

