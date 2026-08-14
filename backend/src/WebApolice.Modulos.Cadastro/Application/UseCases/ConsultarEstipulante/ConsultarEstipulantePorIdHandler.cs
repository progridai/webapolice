using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarEstipulante;

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
