using Microsoft.EntityFrameworkCore;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;
namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;

public sealed class CatalogosSeguroRepository(SeguroDbContext db, OperacaoAuditada auditoria) : ICatalogosSeguro
{
    public async Task<CatalogoPagina<CoberturaDto>> ListarCoberturasAsync(int pagina, int tamanho, string? busca, bool? ativo, CancellationToken ct)
    {
        var q = db.Coberturas.AsNoTracking();
        if (!string.IsNullOrEmpty(busca)) q = q.Where(e => e.Nome!.ToLower().Contains(busca.ToLower()));
        if (ativo.HasValue) q = q.Where(e => e.Ativo == ativo.Value);
        var total = await q.CountAsync(ct);
        return new((await q.OrderBy(e => e.Nome).ThenBy(e => e.Id).Skip((pagina - 1) * tamanho).Take(tamanho).ToListAsync(ct)).Select(Mapear).ToList(), total);
    }
    public async Task<CatalogoPagina<PlanoDto>> ListarPlanosAsync(int pagina, int tamanho, string? busca, bool? ativo, CancellationToken ct)
    {
        var q = db.Planos.AsNoTracking();
        if (!string.IsNullOrEmpty(busca)) q = q.Where(e => e.Nome!.ToLower().Contains(busca.ToLower()));
        if (ativo.HasValue) q = q.Where(e => e.Ativo == ativo.Value);
        var total = await q.CountAsync(ct);
        return new((await q.OrderBy(e => e.Nome).ThenBy(e => e.Id).Skip((pagina - 1) * tamanho).Take(tamanho).ToListAsync(ct)).Select(Mapear).ToList(), total);
    }
    public async Task<CoberturaDto?> ObterCoberturaAsync(Guid id, CancellationToken ct)
    { var e = await db.Coberturas.AsNoTracking().FirstOrDefaultAsync(e => e.PublicId == id, ct); return e is null ? null : Mapear(e); }
    public async Task<PlanoDto?> ObterPlanoAsync(Guid id, CancellationToken ct)
    { var e = await db.Planos.AsNoTracking().FirstOrDefaultAsync(e => e.PublicId == id, ct); return e is null ? null : Mapear(e); }
    public Task<CoberturaDto> SalvarCoberturaAsync(Guid? id, CoberturaDados dados, CancellationToken ct) =>
        auditoria.ExecutarAsync<CoberturaDto>(db, "seguro", "cobertura", id.HasValue ? "alterar" : "criar", async token =>
        {
            var e = id.HasValue ? await db.Coberturas.FirstOrDefaultAsync(e => e.PublicId == id, token) : new Cobertura { PublicId = Guid.NewGuid(), Ativo = true, CreatedAt = DateTime.UtcNow };
            if (e is null) throw new ValidacaoException("Cobertura não encontrada.");
            var antes = id.HasValue ? Mapear(e) : null;
            e.Nome = dados.Nome.Trim(); e.NomeReduzido = Limpar(dados.NomeReduzido); e.Basica = Limpar(dados.Basica); e.Reajuste = dados.Reajuste; e.UpdatedAt = DateTime.UtcNow;
            if (!id.HasValue) db.Coberturas.Add(e);
            await db.SaveChangesAsync(token);
            var depois = Mapear(e); return (depois, e.PublicId.ToString(), (object?)antes, (object?)depois);
        }, ct);
    public Task<PlanoDto> SalvarPlanoAsync(Guid? id, PlanoDados dados, CancellationToken ct) =>
        auditoria.ExecutarAsync<PlanoDto>(db, "seguro", "plano", id.HasValue ? "alterar" : "criar", async token =>
        {
            var e = id.HasValue ? await db.Planos.FirstOrDefaultAsync(e => e.PublicId == id, token) : new Plano { PublicId = Guid.NewGuid(), Ativo = true, CreatedAt = DateTime.UtcNow };
            if (e is null) throw new ValidacaoException("Plano não encontrado.");
            var antes = id.HasValue ? Mapear(e) : null;
            e.Nome = dados.Nome.Trim(); e.Ramo = Limpar(dados.Ramo); e.Paga = dados.Paga; e.Reajuste = dados.Reajuste; e.UpdatedAt = DateTime.UtcNow;
            if (!id.HasValue) db.Planos.Add(e);
            await db.SaveChangesAsync(token);
            var depois = Mapear(e); return (depois, e.PublicId.ToString(), (object?)antes, (object?)depois);
        }, ct);
    public async Task StatusAsync(Guid id, bool ativo, bool cobertura, CancellationToken ct) =>
        await auditoria.ExecutarAsync<bool>(db, "seguro", cobertura ? "cobertura" : "plano", ativo ? "reativar" : "inativar", async token =>
        {
            object antes;
            if (cobertura)
            {
                var e = await db.Coberturas.FirstOrDefaultAsync(e => e.PublicId == id, token) ?? throw new ValidacaoException("Cobertura não encontrada.");
                antes = new { e.PublicId, e.Ativo }; e.Ativo = ativo; e.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var e = await db.Planos.FirstOrDefaultAsync(e => e.PublicId == id, token) ?? throw new ValidacaoException("Plano não encontrado.");
                antes = new { e.PublicId, e.Ativo }; e.Ativo = ativo; e.UpdatedAt = DateTime.UtcNow;
            }
            await db.SaveChangesAsync(token); return (true, id.ToString(), (object?)antes, (object?)new { PublicId = id, Ativo = ativo });
        }, ct);
    public async Task<IReadOnlyList<PlanoCoberturaDto>> CoberturasPlanoAsync(Guid id, CancellationToken ct)
    {
        if (!await db.Planos.AnyAsync(e => e.PublicId == id, ct)) throw new ValidacaoException("Plano não encontrado.");
        return await db.Set<PlanoCoberturaModel>().AsNoTracking().Where(e => e.Plano.PublicId == id).OrderBy(e => e.Cobertura.Nome)
            .Select(e => new PlanoCoberturaDto(e.Cobertura.PublicId, e.Cobertura.Nome!, e.Ativo, e.Cobertura.Ativo, e.PremioTitular, e.PremioConjuge)).ToListAsync(ct);
    }
    public Task VincularAsync(Guid planoId, Guid coberturaId, bool ativo, CancellationToken ct) =>
        PersistirVinculoAsync(planoId, coberturaId, ativo, null, ct);

    public Task SalvarVinculoAsync(Guid planoId, Guid coberturaId, PlanoCoberturaDados dados, CancellationToken ct) =>
        PersistirVinculoAsync(planoId, coberturaId, dados.Ativo, dados, ct);

    private async Task PersistirVinculoAsync(Guid planoId, Guid coberturaId, bool ativo, PremiosCoberturaDados? premios, CancellationToken ct) =>
        await auditoria.ExecutarAsync<bool>(db, "seguro", "plano_cobertura", premios is not null ? "salvar_premios" : ativo ? "reativar" : "inativar", async token =>
        {
            var plano = await db.Planos.FirstOrDefaultAsync(e => e.PublicId == planoId, token) ?? throw new ValidacaoException("Plano não encontrado.");
            var cobertura = await db.Coberturas.FirstOrDefaultAsync(e => e.PublicId == coberturaId, token) ?? throw new ValidacaoException("Cobertura não encontrada.");
            if (ativo && (!plano.Ativo || !cobertura.Ativo)) throw new ValidacaoException("Plano e Cobertura devem estar ativos para vincular.");
            var e = await db.Set<PlanoCoberturaModel>().FirstOrDefaultAsync(e => e.PlanoId == plano.Id && e.CoberturaId == cobertura.Id, token);
            var antes = e is null ? null : Snapshot(e, planoId, coberturaId);
            if (e is null)
            {
                if (premios is null) throw new ValidacaoException("Vínculo não encontrado. Informe os prêmios ao adicionar a Cobertura.");
                e = new() { PlanoId = plano.Id, CoberturaId = cobertura.Id, CreatedAt = DateTimeOffset.UtcNow };
                db.Add(e);
            }
            if (premios is not null)
            {
                WebApolice.Modulos.Seguro.Application.UseCases.Catalogos.CatalogosSeguroHandler.ValidarPremios(premios, true);
                e.PremioTitular = premios.PremioTitular;
                e.PremioConjuge = premios.PremioConjuge;
            }
            if (ativo && (e.PremioTitular is null || e.PremioConjuge is null))
                throw new ValidacaoException("Preencha os prêmios antes de reativar o vínculo.");
            e.Ativo = ativo;
            e.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(token);
            return (true, planoId.ToString(), antes, Snapshot(e, planoId, coberturaId));
        }, ct);
    private static object Snapshot(PlanoCoberturaModel e, Guid planoId, Guid coberturaId) =>
        new { PlanoPublicId = planoId, CoberturaPublicId = coberturaId, e.Ativo, e.PremioTitular, e.PremioConjuge };
    private static string? Limpar(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static CoberturaDto Mapear(Cobertura e) => new(e.PublicId, e.Nome ?? string.Empty, e.NomeReduzido, e.Basica, e.Reajuste, e.Ativo, e.CreatedAt, e.UpdatedAt);
    private static PlanoDto Mapear(Plano e) => new(e.PublicId, e.Nome ?? string.Empty, e.Ramo, e.Paga, e.Reajuste, e.Ativo, e.CreatedAt, e.UpdatedAt);
}
