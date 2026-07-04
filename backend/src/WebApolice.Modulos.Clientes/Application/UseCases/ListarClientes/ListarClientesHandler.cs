using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Domain.Exceptions;

namespace WebApolice.Modulos.Clientes.Application.UseCases.ListarClientes;

public sealed class ListarClientesHandler
{
    private readonly IClientesRepository _repository;

    public ListarClientesHandler(IClientesRepository repository)
    {
        _repository = repository;
    }

    public async Task<ListagemPaginadaResult<ClienteListagemItemResult>> Handle(ListarClientesQuery query, CancellationToken cancellationToken)
    {
        var pagina = query.Pagina > 0 ? query.Pagina : 1;
        var tamanhoPagina = query.TamanhoPagina > 0 ? query.TamanhoPagina : 20;
        if (tamanhoPagina > 100) tamanhoPagina = 100;

        var (itens, totalItens, totalPaginas) = await _repository.ListarPaginadoAsync(
            pagina,
            tamanhoPagina,
            query.Nome,
            query.Cpf,
            query.Status,
            query.OrdenarPor,
            query.Direcao,
            cancellationToken);

        var itensResult = itens.Select(c => new ClienteListagemItemResult(
            c.Id,
            c.Nome,
            "***.***.***-" + c.Cpf.Substring(c.Cpf.Length - 2),
            c.Status.ToString().ToLowerInvariant(),
            c.DataCadastroUtc
        )).ToList();

        return new ListagemPaginadaResult<ClienteListagemItemResult>(
            itensResult,
            pagina,
            tamanhoPagina,
            totalItens,
            totalPaginas
        );
    }
}
