using Microsoft.EntityFrameworkCore;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;

public sealed class ExclusaoVinculosApolice(SeguroDbContext db, OperacaoAuditada auditoria) : IExclusaoVinculosApolice
{
    public async Task ExcluirAsync(Guid apolicePublicId, TipoVinculoApolice tipo, Guid vinculoPublicId, Guid usuarioPublicId, CancellationToken ct)
    {
        if (apolicePublicId == Guid.Empty || vinculoPublicId == Guid.Empty || !Enum.IsDefined(tipo))
            throw new ValidacaoException("Informe a apólice e o vínculo que deseja excluir.");
        await auditoria.ExecutarAsync<bool>(db, "Seguro", "vinculo_apolice", "VINCULO_APOLICE_EXCLUIDO", async token =>
        {
            var apolice = await db.Apolices.FromSqlInterpolated($"SELECT * FROM seguro.apolice WHERE public_id = {apolicePublicId} AND deleted_at IS NULL FOR UPDATE")
                .SingleOrDefaultAsync(token) ?? throw new ValidacaoException("Apólice não encontrada.");
            var exclusao = await ExcluirVinculoAsync(apolice.Id, tipo, vinculoPublicId, token);
            db.ApoliceHistoricos.Add(new()
            {
                ApoliceId = apolice.Id, Acao = "Exclusão de Vínculo",
                Descricao = $"Vínculo {tipo} (PublicId: {vinculoPublicId}) excluído por correção de cadastro.",
                UsuarioPublicId = usuarioPublicId, DataAcao = DateTimeOffset.UtcNow, CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync(token);
            return (true, vinculoPublicId.ToString(),
                (object?)new { ApolicePublicId = apolicePublicId, Tipo = tipo.ToString(), Dados = exclusao.Antes },
                (object?)new { ApolicePublicId = apolicePublicId, Tipo = tipo.ToString(), Dados = exclusao.Depois });
        }, ct);
    }

    private async Task<(object Antes, object Depois)> ExcluirVinculoAsync(long apoliceId, TipoVinculoApolice tipo, Guid id, CancellationToken ct)
    {
        switch (tipo)
        {
            case TipoVinculoApolice.Ramo:
            {
                var e = await Obter(db.ApoliceRamos.Where(e => e.ApoliceId == apoliceId && e.PublicId == id && e.DeletedAt == null), ct);
                if (await db.ApoliceVidas.AnyAsync(v => v.ApoliceId == apoliceId && v.DeletedAt == null, ct) ||
                    await db.Proposta.AnyAsync(p => p.ApoliceId == apoliceId, ct)) Bloquear("vidas ou propostas nesta apólice");
                if (await db.ApoliceProdutos.AnyAsync(p => p.ApoliceId == apoliceId && p.DeletedAt == null, ct) ||
                    await db.ApoliceModuloPlanos.AnyAsync(p => p.ApoliceModulo.ApoliceId == apoliceId && p.DeletedAt == null && p.ApoliceModulo.DeletedAt == null, ct))
                    Bloquear("produtos ou planos de módulos nesta apólice; remova essas configurações primeiro");
                return Marcar(e, id);
            }
            case TipoVinculoApolice.Subgrupo:
            {
                var e = await Obter(db.ApoliceSubgrupos.Where(e => e.ApoliceId == apoliceId && e.PublicId == id && e.DeletedAt == null), ct);
                if (await VidasComUso().AnyAsync(v => v.ApoliceSubgrupoId == e.Id, ct) ||
                    await db.Proposta.AnyAsync(p => p.ApoliceSubgrupoId == e.Id, ct)) Bloquear("vidas ou propostas vinculadas a este subgrupo");
                return Marcar(e, id);
            }
            case TipoVinculoApolice.Subestipulante:
            {
                var e = await Obter(db.ApoliceSubestipulantes.Where(e => e.ApoliceId == apoliceId && e.PublicId == id && e.DeletedAt == null), ct);
                if (await db.Proposta.AnyAsync(p => p.ApoliceId == apoliceId && p.SubestipulanteId == e.SubestipulanteId, ct))
                    Bloquear("propostas vinculadas a este subestipulante");
                return Marcar(e, id);
            }
            case TipoVinculoApolice.Modulo:
            {
                var e = await Obter(db.ApoliceModulos.Where(e => e.ApoliceId == apoliceId && e.PublicId == id && e.DeletedAt == null), ct);
                if (await db.ApoliceModuloPlanos.AnyAsync(p => p.ApoliceModuloId == e.Id && p.DeletedAt == null, ct)) Bloquear("um plano neste módulo; exclua suas coberturas e o plano primeiro");
                await ValidarUsoModulo(e.Id, ct);
                return Marcar(e, id);
            }
            case TipoVinculoApolice.PlanoModulo:
            {
                var e = await Obter(db.ApoliceModuloPlanos.Where(e => e.PublicId == id && e.DeletedAt == null && e.ApoliceModulo.ApoliceId == apoliceId && e.ApoliceModulo.DeletedAt == null), ct);
                if (await db.ApoliceModuloCoberturas.AnyAsync(c => c.ApoliceModuloPlanoId == e.Id && c.DeletedAt == null, ct)) Bloquear("coberturas neste plano; exclua as coberturas primeiro");
                await ValidarUsoModulo(e.ApoliceModuloId, ct);
                return Marcar(e, id);
            }
            case TipoVinculoApolice.CoberturaModulo:
            {
                var e = await Obter(db.ApoliceModuloCoberturas.Include(c => c.Plano).Where(e => e.PublicId == id && e.DeletedAt == null && e.Plano.DeletedAt == null && e.Plano.ApoliceModulo.ApoliceId == apoliceId && e.Plano.ApoliceModulo.DeletedAt == null), ct);
                await ValidarUsoModulo(e.Plano.ApoliceModuloId, ct);
                return Marcar(e, id);
            }
            case TipoVinculoApolice.ProdutoAnterior:
            {
                var e = await Obter(db.ApoliceProdutos.Where(e => e.ApoliceId == apoliceId && e.PublicId == id && e.DeletedAt == null), ct);
                if (await db.ApolicePlanos.AnyAsync(p => p.ApoliceProdutoId == e.Id && p.DeletedAt == null, ct)) Bloquear("planos neste produto; exclua os planos primeiro");
                if (await db.PropostaItems.AnyAsync(i => i.Proposta.ApoliceId == apoliceId && i.ProdutoId == e.ProdutoId, ct)) Bloquear("itens de propostas vinculados a este produto");
                return Marcar(e, id);
            }
            case TipoVinculoApolice.PlanoAnterior:
            {
                var e = await Obter(db.ApolicePlanos.Where(e => e.PublicId == id && e.DeletedAt == null && e.ApoliceProduto!.ApoliceId == apoliceId && e.ApoliceProduto.DeletedAt == null), ct);
                if (await db.ApoliceCoberturas.AnyAsync(c => c.ApolicePlanoId == e.Id && c.DeletedAt == null, ct)) Bloquear("coberturas neste plano; exclua as coberturas primeiro");
                if (await db.PropostaItems.AnyAsync(i => i.Proposta.ApoliceId == apoliceId && i.PlanoId == e.PlanoId, ct)) Bloquear("itens de propostas vinculados a este plano");
                return Marcar(e, id);
            }
            case TipoVinculoApolice.CoberturaAnterior:
            {
                var e = await Obter(db.ApoliceCoberturas.Where(e => e.PublicId == id && e.DeletedAt == null && e.ApolicePlano!.DeletedAt == null && e.ApolicePlano.ApoliceProduto!.ApoliceId == apoliceId && e.ApolicePlano.ApoliceProduto.DeletedAt == null), ct);
                if (await db.PropostaCoberturas.AnyAsync(c => c.Proposta.ApoliceId == apoliceId && (c.CoberturaId == e.CoberturaId || c.CoberturaId == null), ct))
                    Bloquear("coberturas de propostas vinculadas ou sem identificação suficiente para liberar esta cobertura");
                return Marcar(e, id);
            }
            default: throw new ValidacaoException("Tipo de vínculo inválido.");
        }
    }

    private IQueryable<ApoliceVidaModel> VidasComUso() => db.ApoliceVidas.Where(v => v.DeletedAt == null || db.Proposta.Any(p => p.ApoliceVidaId == v.Id));
    private async Task ValidarUsoModulo(long moduloId, CancellationToken ct)
    {
        if (await VidasComUso().AnyAsync(v => v.ApoliceModuloId == moduloId, ct) ||
            await db.Proposta.AnyAsync(p => p.ApoliceModuloId == moduloId, ct) ||
            await db.PropostaItems.AnyAsync(i => i.ApoliceModuloPlano != null && i.ApoliceModuloPlano.ApoliceModuloId == moduloId, ct) ||
            await db.PropostaCoberturas.AnyAsync(c => c.ApoliceModuloCobertura != null && c.ApoliceModuloCobertura.Plano.ApoliceModuloId == moduloId, ct)) Bloquear("vidas ou propostas vinculadas a este módulo");
    }
    private static void Bloquear(string dependencia) => throw new ValidacaoException($"Não é possível excluir: existem {dependencia}. Nenhum vínculo foi removido.");
    private static async Task<T> Obter<T>(IQueryable<T> query, CancellationToken ct) where T : class =>
        await query.SingleOrDefaultAsync(ct) ?? throw new ValidacaoException("Vínculo não encontrado nesta apólice ou já excluído.");
    private (object Antes, object Depois) Marcar<T>(T entidade, Guid id) where T : class
    {
        var entrada = db.Entry(entidade);
        var antes = new { PublicId = id, Ativo = entrada.Property<bool>("Ativo").CurrentValue, DeletedAt = entrada.Property<DateTimeOffset?>("DeletedAt").CurrentValue };
        var agora = DateTimeOffset.UtcNow;
        entrada.Property<bool>("Ativo").CurrentValue = false;
        entrada.Property<DateTimeOffset?>("DeletedAt").CurrentValue = agora;
        entrada.Property<DateTimeOffset>("UpdatedAt").CurrentValue = agora;
        return (antes, new { PublicId = id, Ativo = false, DeletedAt = agora });
    }
}
