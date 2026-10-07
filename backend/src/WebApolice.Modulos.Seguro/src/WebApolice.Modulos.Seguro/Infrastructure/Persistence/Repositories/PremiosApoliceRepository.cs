using Microsoft.EntityFrameworkCore;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Catalogos;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;

public sealed class PremiosApoliceRepository(SeguroDbContext db, OperacaoAuditada auditoria) : IPremiosApolice
{
    public async Task AlterarAsync(Guid apolicePublicId, Guid vinculoPublicId, PremiosCoberturaDados dados, CancellationToken ct) =>
        await auditoria.ExecutarAsync<bool>(db, "seguro", "apolice_cobertura", "alterar_premios", async token =>
        {
            CatalogosSeguroHandler.ValidarPremios(dados, false);
            var e = await db.ApoliceCoberturas.Include(e => e.ApolicePlano!).ThenInclude(e => e.ApoliceProduto!).ThenInclude(e => e.Apolice)
                .FirstOrDefaultAsync(e => e.PublicId == vinculoPublicId && e.ApolicePlano!.ApoliceProduto!.Apolice!.PublicId == apolicePublicId
                    && e.ApolicePlano.ApoliceProduto.Apolice.DeletedAt == null, token)
                ?? throw new ValidacaoException("Cobertura não encontrada nesta Apólice.");
            if (!e.Ativo || !e.ApolicePlano!.Ativo || !e.ApolicePlano.ApoliceProduto!.Ativo || !e.ApolicePlano.ApoliceProduto.Apolice!.Ativo)
                throw new ValidacaoException("A Apólice e seus vínculos devem estar ativos para ajustar os prêmios.");
            var antes = new { e.PublicId, e.PremioTitularOverride, e.PremioConjugeOverride };
            e.PremioTitularOverride = dados.PremioTitular;
            e.PremioConjugeOverride = dados.PremioConjuge;
            e.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(token);
            return (true, e.PublicId.ToString(), (object?)antes, (object?)new { e.PublicId, e.PremioTitularOverride, e.PremioConjugeOverride });
        }, ct);
}
