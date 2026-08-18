using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.SharedKernel.Application;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ListarCooperados;

public sealed record ListarCooperadosQuery(
    int Pagina = 1,
    int TamanhoPagina = 10,
    string? TermoBusca = null,
    short? Tipo = null
);

public sealed class ListarCooperadosHandler
{
    private readonly ICooperadosQueries _queries;

    public ListarCooperadosHandler(ICooperadosQueries queries)
    {
        _queries = queries;
    }

    public async Task<WebApolice.Modulos.Cadastro.Application.UseCases.ListarClientes.ListagemPaginadaResult<CooperadoListDto>> Handle(ListarCooperadosQuery query, CancellationToken cancellationToken)
    {
        return await _queries.ListarAsync(
            query.Pagina,
            query.TamanhoPagina,
            query.TermoBusca,
            query.Tipo,
            cancellationToken
        );
    }
}
