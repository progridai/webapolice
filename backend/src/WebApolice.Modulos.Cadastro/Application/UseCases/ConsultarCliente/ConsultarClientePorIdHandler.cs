using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;

using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarCliente;

public sealed class ConsultarClientePorIdHandler
{
    private readonly IClientesQueries _queries;
    private readonly WebApolice.Modulos.Seguranca.Infrastructure.Persistence.SegurancaDbContext _segurancaDbContext;

    public ConsultarClientePorIdHandler(IClientesQueries queries, WebApolice.Modulos.Seguranca.Infrastructure.Persistence.SegurancaDbContext segurancaDbContext)
    {
        _queries = queries;
        _segurancaDbContext = segurancaDbContext;
    }

    public async Task<ConsultarClienteResult> Handle(ConsultarClientePorIdQuery query, CancellationToken cancellationToken)
    {
        var result = await _queries.ObterDetalheAsync(query.Id, cancellationToken);
        if (result == null)
            throw new ClienteNaoEncontradoException("Cliente não encontrado.");

        var recursoRe = await _segurancaDbContext.Recursos
            .Include(r => r.Modulo)
            .Where(r => r.Codigo == "RE" && r.Modulo.Codigo == "CLIENTES")
            .FirstOrDefaultAsync(cancellationToken);

        if (recursoRe == null || !recursoRe.Habilitado || !recursoRe.Ativo || !recursoRe.Modulo.Habilitado || !recursoRe.Modulo.Ativo)
        {
            result = result with { Re = null };
        }

        return result;
    }
}
