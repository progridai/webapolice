using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;


namespace WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarCliente;

public sealed class ConsultarClientePorIdHandler
{
    private readonly IClientesQueries _queries;
    private readonly WebApolice.Modulos.Seguranca.Contracts.IDisponibilidadeCatalogo _catalogo;

    public ConsultarClientePorIdHandler(IClientesQueries queries, WebApolice.Modulos.Seguranca.Contracts.IDisponibilidadeCatalogo catalogo)
    {
        _queries = queries;
        _catalogo = catalogo;
    }

    public async Task<ConsultarClienteResult> Handle(ConsultarClientePorIdQuery query, CancellationToken cancellationToken)
    {
        var result = await _queries.ObterDetalheAsync(query.Id, cancellationToken);
        if (result == null)
            throw new ClienteNaoEncontradoException("Cliente não encontrado.");

        if (!await _catalogo.RecursoDisponivelAsync("CLIENTES", "RE", cancellationToken))
        {
            result = result with { Re = null };
        }

        return result;
    }
}
