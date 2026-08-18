using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Models;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ListarPropostas;

public class ObterPropostasHandler : IRequestHandler<ObterPropostasQuery, PagedResult<PropostaDto>>
{
    private readonly SeguroDbContext _dbContext;

    public ObterPropostasHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<PropostaDto>> Handle(ObterPropostasQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Proposta
            .Include(p => p.Status)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.TermoBusca))
        {
            query = query.Where(p => p.Numero != null && p.Numero.Contains(request.TermoBusca));
        }

        var totalRegistros = await query.CountAsync(cancellationToken);

        var propostas = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Pagina - 1) * request.TamanhoPagina)
            .Take(request.TamanhoPagina)
            .Select(p => new PropostaDto
            {
                PublicId = p.PublicId,
                Numero = p.Numero,
                IntegradaApolice = p.ApoliceId != null && p.ApoliceVidaId != null,
                PremioLiquido = p.PremioLiquido,
                DataInclusao = p.DataInclusao,
                Status = p.Status.Nome
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<PropostaDto> { Items = propostas, TotalCount = totalRegistros, Page = request.Pagina, PageSize = request.TamanhoPagina };
    }
}
