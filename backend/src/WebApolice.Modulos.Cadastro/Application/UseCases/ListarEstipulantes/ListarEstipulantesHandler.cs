using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarEstipulante;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ListarEstipulantes;

public sealed class ListarEstipulantesHandler
{
    private readonly IEstipulantesQueries _queries;

    public ListarEstipulantesHandler(IEstipulantesQueries queries)
    {
        _queries = queries;
    }

    public async Task<ListagemPaginadaResult<EstipulanteDetalheResult>> Handle(ListarEstipulantesQuery query, CancellationToken cancellationToken)
    {
        var pagina = query.Pagina > 0 ? query.Pagina : 1;
        var tamanhoPagina = query.TamanhoPagina > 0 ? query.TamanhoPagina : 20;
        if (tamanhoPagina > 100) tamanhoPagina = 100;

        var (itens, totalItens, totalPaginas) = await _queries.ListarPaginadoAsync(
            pagina,
            tamanhoPagina,
            query.Nome,
            query.Cnpj,
            cancellationToken);

        return new ListagemPaginadaResult<EstipulanteDetalheResult>(
            itens,
            pagina,
            tamanhoPagina,
            totalItens,
            totalPaginas
        );
    }
}
