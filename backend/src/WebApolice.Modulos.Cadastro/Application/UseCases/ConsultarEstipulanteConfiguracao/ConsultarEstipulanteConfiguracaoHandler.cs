using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarEstipulanteConfiguracao;

public sealed record ConsultarEstipulanteConfiguracaoQuery(Guid PublicId);

public sealed class ConsultarEstipulanteConfiguracaoHandler
{
    private readonly IEstipulantesQueries _queries;

    public ConsultarEstipulanteConfiguracaoHandler(IEstipulantesQueries queries)
    {
        _queries = queries;
    }

    public async Task<EstipulanteConfiguracaoResult?> Handle(ConsultarEstipulanteConfiguracaoQuery query, CancellationToken cancellationToken)
    {
        return await _queries.ObterConfiguracaoPorPublicIdAsync(query.PublicId, cancellationToken) as EstipulanteConfiguracaoResult;
    }
}
