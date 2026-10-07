using Microsoft.EntityFrameworkCore;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.PlanoModulo;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;

public sealed class PlanoModuloApoliceRepository(SeguroDbContext db, OperacaoAuditada auditoria) : IPlanoModuloApolice
{
    public async Task<CatalogoPagina<CoberturaModuloOpcao>> OpcoesCoberturasAsync(Guid apolice, Guid modulo, int pagina, int tamanho, CancellationToken ct)
    {
        if (!await db.ApoliceModulos.AsNoTracking().AnyAsync(e => e.PublicId == modulo && e.DeletedAt == null && e.Apolice!.PublicId == apolice && e.Apolice.DeletedAt == null, ct))
            throw new ValidacaoException("Módulo não encontrado nesta Apólice.");
        var query = db.Coberturas.AsNoTracking().Where(c => c.Ativo);
        return new(await query.OrderBy(c => c.Nome).ThenBy(c => c.PublicId).Skip((pagina - 1) * tamanho).Take(tamanho)
            .Select(c => new CoberturaModuloOpcao(c.PublicId, c.Nome!)).ToListAsync(ct), await query.CountAsync(ct));
    }

    public async Task<ConfiguracaoPlanoModuloDto> ObterAsync(Guid apolice, Guid modulo, CancellationToken ct)
    {
        var vinculo = await db.ApoliceModulos.AsNoTracking().Include(e => e.Apolice)
            .SingleOrDefaultAsync(e => e.PublicId == modulo && e.DeletedAt == null && e.Apolice!.PublicId == apolice && e.Apolice.DeletedAt == null, ct)
            ?? throw new ValidacaoException("Módulo não encontrado nesta Apólice.");
        var plano = await db.ApoliceModuloPlanos.AsNoTracking().Include(e => e.Coberturas).ThenInclude(e => e.Cobertura)
            .SingleOrDefaultAsync(e => e.ApoliceModuloId == vinculo.Id, ct);
        return new(modulo, vinculo.Ativo && vinculo.Apolice!.Ativo, plano is null ? null : Dto(plano));
    }

    public Task<PlanoModuloDto> SalvarPlanoAsync(Guid apolice, Guid modulo, PlanoModuloDados dados, CancellationToken ct) =>
        auditoria.ExecutarAsync<PlanoModuloDto>(db, "seguro", "apolice_modulo_plano", "salvar", async token =>
        {
            // Também valida chamadas internas feitas diretamente ao port.
            PlanoModuloApoliceHandler.ValidarPlano(dados);
            var vinculo = await BloquearVinculoAsync(apolice, modulo, token);
            var e = await db.ApoliceModuloPlanos.Include(e => e.Coberturas).ThenInclude(e => e.Cobertura).SingleOrDefaultAsync(e => e.ApoliceModuloId == vinculo.Id, token);
            object? antes = e is null ? null : Snapshot(apolice, modulo, Dto(e));
            if (e is null)
            {
                e = new() { PublicId = Guid.NewGuid(), ApoliceModuloId = vinculo.Id, CreatedAt = DateTimeOffset.UtcNow };
                db.ApoliceModuloPlanos.Add(e);
            }
            e.Nome = dados.Nome.Trim(); e.Ramo = Limpar(dados.Ramo); e.Paga = dados.Paga;
            e.Reajuste = dados.Reajuste; e.Ativo = dados.Ativo; e.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(token);
            var dto = Dto(e);
            return (dto, e.PublicId.ToString(), antes, (object?)Snapshot(apolice, modulo, dto));
        }, ct);

    public Task<CoberturaModuloDto> SalvarCoberturaAsync(Guid apolice, Guid modulo, Guid? cobertura, CoberturaModuloDados dados, CancellationToken ct) =>
        auditoria.ExecutarAsync<CoberturaModuloDto>(db, "seguro", "apolice_modulo_cobertura", cobertura is null ? "criar" : "alterar", async token =>
        {
            PlanoModuloApoliceHandler.ValidarCobertura(dados);
            var vinculo = await BloquearVinculoAsync(apolice, modulo, token);
            var plano = await db.ApoliceModuloPlanos.SingleOrDefaultAsync(e => e.ApoliceModuloId == vinculo.Id, token)
                ?? throw new ValidacaoException("Cadastre o Plano deste Módulo antes de criar Coberturas.");
            if (!plano.Ativo) throw new ValidacaoException("O Plano deve estar ativo para alterar suas Coberturas.");
            var e = cobertura is null ? null : await db.ApoliceModuloCoberturas
                .SingleOrDefaultAsync(e => e.PublicId == cobertura && e.ApoliceModuloPlanoId == plano.Id, token)
                ?? throw new ValidacaoException("Cobertura não encontrada no Plano deste Módulo.");
            var cadastros = await db.Coberturas.FromSqlInterpolated($"SELECT * FROM seguro.cobertura WHERE public_id = {dados.CoberturaPublicId} FOR UPDATE").ToListAsync(token);
            var cadastro = cadastros.SingleOrDefault() ?? throw new ValidacaoException("Cobertura não encontrada no cadastro.");
            if (e is not null && e.CoberturaId != cadastro.Id)
                throw new ValidacaoException("A Cobertura de um vínculo existente não pode ser substituída. Crie outro vínculo.");
            if ((e is null || (!e.Ativo && dados.Ativo)) && !cadastro.Ativo)
                throw new ValidacaoException("A Cobertura do cadastro deve estar ativa para vincular ou reativar.");
            if (e is null && await db.ApoliceModuloCoberturas.AnyAsync(c => c.ApoliceModuloPlanoId == plano.Id && c.CoberturaId == cadastro.Id, token))
                throw new ValidacaoException("Esta Cobertura já está vinculada ao Plano. Edite ou reative o vínculo existente.");
            if (e is not null) e.Cobertura = cadastro;
            object? antes = e is null ? null : Snapshot(apolice, modulo, Dto(e));
            if (e is null)
            {
                e = new() { PublicId = Guid.NewGuid(), ApoliceModuloPlanoId = plano.Id, CoberturaId = cadastro.Id,
                    Cobertura = cadastro, CreatedAt = DateTimeOffset.UtcNow };
                db.ApoliceModuloCoberturas.Add(e);
            }
            e.PremioTitular = dados.PremioTitular!.Value;
            e.PremioConjuge = dados.PremioConjuge!.Value;
            e.Ativo = dados.Ativo; e.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(token);
            var dto = Dto(e);
            return (dto, e.PublicId.ToString(), antes, (object?)Snapshot(apolice, modulo, dto));
        }, ct);

    private async Task<ApoliceModuloModel> BloquearVinculoAsync(Guid apolice, Guid modulo, CancellationToken ct)
    {
        // Serializa alterações do vínculo e a criação do único Plano. Os locks
        // também impedem que uma inativação concorrente passe pela validação.
        var apolices = await db.Apolices.FromSqlInterpolated($"SELECT * FROM seguro.apolice WHERE public_id = {apolice} AND deleted_at IS NULL FOR UPDATE").ToListAsync(ct);
        var a = apolices.SingleOrDefault() ?? throw new ValidacaoException("Apólice não encontrada.");
        var vinculos = await db.ApoliceModulos.FromSqlInterpolated($"SELECT * FROM seguro.apolice_modulo WHERE public_id = {modulo} AND apolice_id = {a.Id} AND deleted_at IS NULL FOR UPDATE").ToListAsync(ct);
        var vinculo = vinculos.SingleOrDefault() ?? throw new ValidacaoException("Módulo não encontrado nesta Apólice.");
        if (!a.Ativo || !vinculo.Ativo) throw new ValidacaoException("A Apólice e o vínculo do Módulo devem estar ativos para alterar o Plano e suas Coberturas.");
        return vinculo;
    }

    private static string? Limpar(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
    private static object Snapshot(Guid apolice, Guid modulo, object dados) => new { ApolicePublicId = apolice, ApoliceModuloPublicId = modulo, Dados = dados };
    private static CoberturaModuloDto Dto(ApoliceModuloCoberturaModel e) => new(e.PublicId, e.Cobertura.PublicId, e.Cobertura.Ativo, e.Cobertura.Nome!, e.Cobertura.NomeReduzido, e.Cobertura.Basica, e.Cobertura.Reajuste, e.PremioTitular, e.PremioConjuge, e.Ativo);
    private static PlanoModuloDto Dto(ApoliceModuloPlanoModel e) => new(e.PublicId, e.Nome, e.Ramo, e.Paga, e.Reajuste, e.Ativo,
        e.Coberturas.OrderBy(e => e.Cobertura.Nome).ThenBy(e => e.PublicId).Select(Dto).ToList());

}
