using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.SharedKernel.Application.Models;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ListarPropostas;

public class ObterPropostasHandler : IRequestHandler<ObterPropostasQuery, PagedResult<PropostaDto>>
{
    private readonly IPropostasRepository _repository;

    public ObterPropostasHandler(IPropostasRepository dbContext)
    {
        _repository = dbContext;
    }

    public async Task<PagedResult<PropostaDto>> Handle(ObterPropostasQuery request, CancellationToken cancellationToken)
    {
        if (request.Pagina < 1 || request.TamanhoPagina is < 1 or > 100 ||
            ((long)request.Pagina - 1) * request.TamanhoPagina > int.MaxValue)
            throw new WebApolice.SharedKernel.Application.Exceptions.ValidacaoException("Paginação inválida: página >= 1 e tamanho entre 1 e 100.");
        return await _repository.ListarAsync(request, cancellationToken);
    }
}
