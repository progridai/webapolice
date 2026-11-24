using WebApolice.Modulos.Cadastro.Contracts;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;

public sealed class ParcelasPropostasRepository(SeguroDbContext db, IClientesPropostas clientes) : IParcelasPropostasRepository
{
    public Task<ReferenciasPropostas> ObterReferenciasAsync(IReadOnlyList<Propostum> propostas, CancellationToken ct) =>
        clientes.ConsultarReferenciasAsync(propostas.Select(p => p.ClienteId).Distinct().ToArray(), propostas.Select(p => p.ClienteVinculoId).Distinct().ToArray(), ct);
    public Task<PropostaFinanceiro?> ObterPlanoAsync(long propostaId, CancellationToken ct) => db.PropostasFinanceiro.Include(f => f.ApoliceRamo).ThenInclude(r => r.Ramo).SingleOrDefaultAsync(f => f.PropostaId == propostaId, ct);
    public async Task<IReadOnlyList<PropostaFinanceiro>> ObterPlanosAsync(long apoliceId, CancellationToken ct) =>
        await db.PropostasFinanceiro.Include(f => f.Proposta).ThenInclude(p => p.ApoliceVida)
            .Include(f => f.Proposta).ThenInclude(p => p.PropostaItems)
            .Include(f => f.Proposta).ThenInclude(p => p.PropostaCoberturas)
            .Where(f => f.Proposta.ApoliceId == apoliceId).OrderBy(f => f.Id).AsSplitQuery().ToListAsync(ct);
    public async Task<IReadOnlyList<Propostum>> ObterPropostasAsync(long apoliceId, CancellationToken ct) =>
        await db.Proposta.Include(p => p.ApoliceVida).Where(p => p.ApoliceId == apoliceId).OrderBy(p => p.Id).ToListAsync(ct);
    public async Task<IReadOnlyList<PropostaMovimento>> ObterMovimentosAsync(long apoliceId, CancellationToken ct) =>
        await db.PropostaMovimentos.Include(m => m.Proposta).Where(m => m.Proposta != null && m.Proposta.ApoliceId == apoliceId).OrderBy(m => m.Id).ToListAsync(ct);
    public async Task<IReadOnlyList<ParcelaCompetencia>> ObterCompetenciasAsync(long apoliceId, CancellationToken ct) =>
        await db.ParcelasCompetencias.Where(c => c.ApoliceId == apoliceId).OrderBy(c => c.Competencia).ToListAsync(ct);
    public Task<PropostaMovimento?> ObterMovimentoAsync(Guid publicId, CancellationToken ct) => db.PropostaMovimentos.Include(m => m.Proposta).SingleOrDefaultAsync(m => m.PublicId == publicId, ct);
    public void Adicionar(PropostaFinanceiro plano) => db.PropostasFinanceiro.Add(plano);
    public void Adicionar(PropostaMovimento movimento) => db.PropostaMovimentos.Add(movimento);
    public void Adicionar(ParcelaCompetencia competencia) => db.ParcelasCompetencias.Add(competencia);
}
