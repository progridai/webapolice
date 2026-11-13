using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence.Queries;

public sealed class ClienteVinculosUso(SeguroDbContext db) : IClienteVinculosUso
{
    public async Task<IReadOnlyDictionary<long, IReadOnlyList<ApoliceVinculoCliente>>> ListarApolicesAsync(
        long clienteId, IReadOnlyCollection<long> vinculoIds, CancellationToken ct)
    {
        if (vinculoIds.Count == 0) return new Dictionary<long, IReadOnlyList<ApoliceVinculoCliente>>();
        var participacoes = await (from v in db.ApoliceVidas.AsNoTracking()
                                   join a in db.Apolices on v.ApoliceId equals a.Id
                                   where v.ClienteId == clienteId && v.ClienteVinculoId.HasValue &&
                                       vinculoIds.Contains(v.ClienteVinculoId.Value) && v.DeletedAt == null && a.DeletedAt == null
                                   select new { VinculoId = v.ClienteVinculoId!.Value, a.Id, a.PublicId, a.Nome,
                                       a.Ativo, ParticipacaoAtiva = v.Ativo }).ToListAsync(ct);
        var apoliceIds = participacoes.Select(p => p.Id).Distinct().ToArray();
        var numeros = await db.ApoliceRamos.AsNoTracking()
            .Where(r => apoliceIds.Contains(r.ApoliceId) && r.DeletedAt == null && r.NumeroApolice != null && r.NumeroApolice != "")
            .Select(r => new { r.ApoliceId, r.NumeroApolice }).Distinct().ToListAsync(ct);
        var numerosPorApolice = numeros.ToLookup(r => r.ApoliceId, r => r.NumeroApolice!);
        return participacoes.GroupBy(p => p.VinculoId).ToDictionary(g => g.Key,
            g => (IReadOnlyList<ApoliceVinculoCliente>)g.GroupBy(p => p.Id).Select(p =>
            {
                var apolice = p.First();
                return new ApoliceVinculoCliente(apolice.PublicId, apolice.Nome,
                    numerosPorApolice[apolice.Id].Order(StringComparer.Ordinal).ToArray(),
                    apolice.Ativo, p.Any(v => v.ParticipacaoAtiva));
            }).OrderBy(a => a.Nome, StringComparer.Ordinal).ThenBy(a => a.PublicId).ToArray());
    }

    public async Task<IReadOnlyCollection<long>> ListarEmUsoAsync(IReadOnlyCollection<long> vinculoIds, CancellationToken ct)
    {
        if (vinculoIds.Count == 0) return [];
        var vidas = await db.ApoliceVidas.Where(v => v.ClienteVinculoId.HasValue && vinculoIds.Contains(v.ClienteVinculoId.Value))
            .Select(v => v.ClienteVinculoId!.Value).Distinct().ToListAsync(ct);
        var propostas = await db.Proposta.Where(p => vinculoIds.Contains(p.ClienteVinculoId))
            .Select(p => p.ClienteVinculoId).Distinct().ToListAsync(ct);
        return vidas.Concat(propostas).Distinct().ToArray();
    }
    public async Task<bool> EstaEmUsoAsync(long vinculoId, CancellationToken ct) =>
        await db.ApoliceVidas.AnyAsync(v => v.ClienteVinculoId == vinculoId, ct) ||
        await db.Proposta.AnyAsync(p => p.ClienteVinculoId == vinculoId, ct);
}
