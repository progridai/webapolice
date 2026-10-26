using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ListarPropostas;
using WebApolice.SharedKernel.Application.Models;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;

public sealed class PropostasRepository(SeguroDbContext _dbContext) : IPropostasRepository
{
    public Task<PropostaStatus?> ObterStatusAsync(string codigo, CancellationToken ct) => _dbContext.PropostaStatuses.SingleOrDefaultAsync(s => s.Codigo == codigo, ct);
    public Task<ApoliceModel?> ObterApoliceAsync(Guid publicId, CancellationToken ct) => _dbContext.Apolices.FirstOrDefaultAsync(a => a.PublicId == publicId, ct);
    public Task<ApoliceVidaModel?> ObterVidaAsync(Guid publicId, CancellationToken ct) => _dbContext.ApoliceVidas.FirstOrDefaultAsync(v => v.PublicId == publicId, ct);
    public Task<Propostum?> ObterAsync(Guid publicId, CancellationToken ct) => _dbContext.Proposta.FirstOrDefaultAsync(p => p.PublicId == publicId, ct);
    public Task<ApoliceProdutoModel?> ObterProdutoPermitidoAsync(long? apoliceId, long produtoId, CancellationToken ct) => _dbContext.ApoliceProdutos.FirstOrDefaultAsync(p => p.ApoliceId == apoliceId && p.ProdutoId == produtoId, ct);
    public Task<ApolicePlanoModel?> ObterPlanoPermitidoAsync(long apoliceProdutoId, long planoId, CancellationToken ct) => _dbContext.ApolicePlanos.FirstOrDefaultAsync(p => p.ApoliceProdutoId == apoliceProdutoId && p.PlanoId == planoId, ct);
    public void Adicionar(Propostum proposta) => _dbContext.Proposta.Add(proposta);
    public void AdicionarItem(PropostaItem item) => _dbContext.PropostaItems.Add(item);
    public async Task SalvarAlteracoesAsync(CancellationToken ct) => await _dbContext.SaveChangesAsync(ct);
    public async Task<PagedResult<PropostaDto>> ListarAsync(ObterPropostasQuery request, CancellationToken cancellationToken)
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
            .ThenByDescending(p => p.Id)
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
