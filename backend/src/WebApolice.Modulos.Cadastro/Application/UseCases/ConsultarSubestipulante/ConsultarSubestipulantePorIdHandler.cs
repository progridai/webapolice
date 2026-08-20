using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarSubestipulante;

public sealed class ConsultarSubestipulantePorIdHandler
{
    private readonly ISubestipulantesQueries _queries;

    public ConsultarSubestipulantePorIdHandler(ISubestipulantesQueries queries)
    {
        _queries = queries;
    }

    public async Task<SubestipulanteDetalheResult?> Handle(ObterSubestipulantePorPublicIdQuery query, CancellationToken cancellationToken)
    {
        return await _queries.ObterPorPublicIdAsync(query.PublicId, cancellationToken);
    }
}
