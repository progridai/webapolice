using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Estipulantes.Application.Ports;

namespace WebApolice.Modulos.Estipulantes.Application.UseCases.ConsultarEstipulante;

public sealed record ConsultarEstipulantePorIdQuery(Guid PublicId);

public sealed class ConsultarEstipulantePorIdHandler
{
    private readonly IEstipulantesQueries _queries;

    public ConsultarEstipulantePorIdHandler(IEstipulantesQueries queries)
    {
        _queries = queries;
    }

    public async Task<EstipulanteDetalheResult?> Handle(ConsultarEstipulantePorIdQuery query, CancellationToken cancellationToken)
    {
        return await _queries.ObterPorPublicIdAsync(query.PublicId, cancellationToken);
    }
}
