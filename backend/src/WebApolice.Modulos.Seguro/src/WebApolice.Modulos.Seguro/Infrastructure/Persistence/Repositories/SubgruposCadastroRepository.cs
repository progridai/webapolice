using Microsoft.EntityFrameworkCore;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Financeiro.Contracts;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;
namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;

public sealed class SubgruposCadastroRepository(SeguroDbContext db, IConveniosCobrancaConsulta convenios, OperacaoAuditada auditoria) : ISubgruposCadastro
{
    public Task<Guid> SalvarAsync(Guid apoliceId, Guid? subgrupoId, string nome, string? observacao, Guid? convenioId, CancellationToken ct) =>
        auditoria.ExecutarAsync<Guid>(db, "seguro", "apolice_subgrupo", subgrupoId.HasValue ? "alterar" : "criar", async token =>
        {
            var apolice = await db.Apolices.FirstOrDefaultAsync(a => a.PublicId == apoliceId && a.DeletedAt == null, token) ?? throw new ValidacaoException("Apólice não encontrada.");
            var e = subgrupoId.HasValue ? await db.ApoliceSubgrupos.FirstOrDefaultAsync(e => e.PublicId == subgrupoId && e.ApoliceId == apolice.Id && e.DeletedAt == null, token) : new ApoliceSubgrupoModel { PublicId = Guid.NewGuid(), ApoliceId = apolice.Id, CreatedAt = DateTimeOffset.UtcNow };
            if (e is null) throw new ValidacaoException("Subgrupo não encontrado nesta Apólice.");
            var antes = subgrupoId.HasValue ? await SnapshotAsync(e, token) : null;
            if (convenioId.HasValue)
            {
                var convenio = await convenios.ObterAsync(convenioId.Value, token) ?? throw new ValidacaoException("Convênio de Cobrança não encontrado.");
                // Existing inactive references can be retained, but cannot be assigned anew.
                if (!convenio.Ativo && e.ConvenioCobrancaId != convenio.Id) throw new ValidacaoException("Convênio de Cobrança está inativo.");
                e.ConvenioCobrancaId = convenio.Id;
            }
            else if (!subgrupoId.HasValue || e.ConvenioCobrancaId.HasValue)
                throw new ValidacaoException("O Convênio de Cobrança é obrigatório; um vínculo existente não pode ser removido.");
            e.Nome = nome.Trim(); e.Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim(); e.UpdatedAt = DateTimeOffset.UtcNow;
            if (!subgrupoId.HasValue) db.ApoliceSubgrupos.Add(e);
            await db.SaveChangesAsync(token);
            return (e.PublicId, e.PublicId.ToString(), antes, await SnapshotAsync(e, token));
        }, ct);
    public async Task InativarAsync(Guid apoliceId, Guid subgrupoId, CancellationToken ct) =>
        await auditoria.ExecutarAsync<bool>(db, "seguro", "apolice_subgrupo", "inativar", async token =>
        {
            var e = await db.ApoliceSubgrupos.FirstOrDefaultAsync(e => e.PublicId == subgrupoId && e.Apolice!.PublicId == apoliceId && e.Apolice.DeletedAt == null && e.DeletedAt == null, token) ?? throw new ValidacaoException("Subgrupo não encontrado nesta Apólice.");
            if (!e.Ativo) throw new ValidacaoException("O Subgrupo já está inativo.");
            var antes = await SnapshotAsync(e, token); e.Ativo = false; e.UpdatedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(token);
            return (true, e.PublicId.ToString(), antes, await SnapshotAsync(e, token));
        }, ct);
    private async Task<object> SnapshotAsync(ApoliceSubgrupoModel e, CancellationToken ct)
    {
        var refs = e.ConvenioCobrancaId.HasValue ? await convenios.ObterPorIdsAsync(new[] { e.ConvenioCobrancaId.Value }, ct) : Array.Empty<ConvenioCobrancaReferencia>();
        return new { e.PublicId, e.Nome, e.Observacao, e.Ativo, ConvenioCobrancaPublicId = refs.FirstOrDefault()?.PublicId };
    }
}
