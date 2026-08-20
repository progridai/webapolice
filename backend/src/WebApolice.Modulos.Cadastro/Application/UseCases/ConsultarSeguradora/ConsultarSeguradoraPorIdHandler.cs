using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarSeguradora;

public sealed class ConsultarSeguradoraPorIdHandler
{
    private readonly ISeguradorasQueries _queries;

    public ConsultarSeguradoraPorIdHandler(ISeguradorasQueries queries)
    {
        _queries = queries;
    }

    public async Task<SeguradoraDetalheResult?> Handle(ObterSeguradoraPorIdQuery query, CancellationToken cancellationToken)
    {
        return await _queries.ObterPorPublicIdAsync(query.PublicId, cancellationToken);
    }
}
