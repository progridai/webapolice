using Microsoft.EntityFrameworkCore;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Financeiro.Application.Ports;
using WebApolice.Modulos.Financeiro.Contracts;
using WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence;
using WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;
namespace WebApolice.Modulos.Financeiro.Infrastructure;

public sealed class ConveniosCobrancaRepository(FinanceiroDbContext db, OperacaoAuditada auditoria) : IConveniosCobrancaCadastro, IConveniosCobrancaConsulta
{
    public async Task<IReadOnlyList<BancoOpcao>> BancosAsync(CancellationToken ct) =>
        await db.Database.SqlQueryRaw<BancoOpcao>("SELECT codigo AS \"Codigo\", nome AS \"Nome\" FROM core.banco WHERE codigo IS NOT NULL AND nome IS NOT NULL ORDER BY nome").ToListAsync(ct);
    public Task<ConvenioCobrancaReferencia?> ObterAsync(Guid id, CancellationToken ct) =>
        db.ConvenioCobrancas.AsNoTracking().Where(e => e.PublicId == id).Select(e => new ConvenioCobrancaReferencia(e.Id, e.PublicId, e.Nome!, e.Ativo)).FirstOrDefaultAsync(ct);
    public async Task<IReadOnlyList<ConvenioCobrancaReferencia>> ObterPorIdsAsync(IReadOnlyList<long> ids, CancellationToken ct) =>
        await db.ConvenioCobrancas.AsNoTracking().Where(e => ids.Contains(e.Id)).Select(e => new ConvenioCobrancaReferencia(e.Id, e.PublicId, e.Nome!, e.Ativo)).ToListAsync(ct);
    public async Task<IReadOnlyList<ConvenioCobrancaOpcao>> OpcoesAsync(CancellationToken ct) =>
        await db.ConvenioCobrancas.AsNoTracking().Where(e => e.Ativo).OrderBy(e => e.Nome).Select(e => new ConvenioCobrancaOpcao(e.PublicId, e.Nome!, e.Ativo)).ToListAsync(ct);
    private async Task<string?> BancoCodigoAsync(long? bancoId, CancellationToken ct)
    {
        if (bancoId is null) return null;
        return await db.Database.SqlQuery<string>($"SELECT codigo AS \"Value\" FROM core.banco WHERE id = {bancoId.Value}").FirstOrDefaultAsync(ct);
    }
    public async Task<ConveniosPagina> ListarAsync(int pagina, int tamanho, string? busca, bool? ativo, CancellationToken ct)
    {
        var query = db.ConvenioCobrancas.AsNoTracking();
        if (!string.IsNullOrEmpty(busca)) query = query.Where(e => e.Nome!.ToLower().Contains(busca.ToLower()));
        if (ativo.HasValue) query = query.Where(e => e.Ativo == ativo.Value);
        var total = await query.CountAsync(ct);
        var entities = await query.OrderBy(e => e.Nome).ThenBy(e => e.Id).Skip((pagina - 1) * tamanho).Take(tamanho).ToListAsync(ct);
        var bancoIds = entities.Where(e => e.BancoId.HasValue).Select(e => e.BancoId!.Value).Distinct().ToArray();
        var bancos = bancoIds.Length == 0 ? new List<BancoReferenciaSql>() : await db.Database
            .SqlQuery<BancoReferenciaSql>($"SELECT id AS \"Id\", codigo AS \"Codigo\" FROM core.banco WHERE id = ANY({bancoIds})")
            .ToListAsync(ct);
        var porId = bancos.ToDictionary(e => e.Id, e => e.Codigo);
        var items = entities.Select(e => Mapear(e, e.BancoId.HasValue ? porId.GetValueOrDefault(e.BancoId.Value) : null)).ToList();
        return new(items, total);
    }
    public async Task<ConvenioCobrancaDto?> ConsultarAsync(Guid id, CancellationToken ct)
    {
        var e = await db.ConvenioCobrancas.AsNoTracking().FirstOrDefaultAsync(e => e.PublicId == id, ct);
        return e is null ? null : Mapear(e, await BancoCodigoAsync(e.BancoId, ct));
    }
    public Task<ConvenioCobrancaDto> SalvarAsync(Guid? id, ConvenioCobrancaDados dados, CancellationToken ct) =>
        auditoria.ExecutarAsync<ConvenioCobrancaDto>(db, "financeiro", "convenio_cobranca", id.HasValue ? "alterar" : "criar", async token =>
        {
            var entity = id.HasValue ? await db.ConvenioCobrancas.FirstOrDefaultAsync(e => e.PublicId == id.Value, token) : new ConvenioCobranca { PublicId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
            if (entity is null) throw new ValidacaoException("Convênio não encontrado.");
            var antes = id.HasValue ? Mapear(entity, await BancoCodigoAsync(entity.BancoId, token)) : null;
            long? bancoId = null;
            var codigo = NullIfEmpty(dados.BancoCodigo);
            if (codigo is not null)
            {
                bancoId = await db.Database.SqlQuery<long?>($"SELECT id AS \"Value\" FROM core.banco WHERE codigo = {codigo}").FirstOrDefaultAsync(token);
                if (bancoId is null) throw new ValidacaoException("Banco não encontrado.");
            }
            entity.BancoId = bancoId;
            entity.Nome = dados.Nome.Trim();
            entity.Agencia = NullIfEmpty(dados.Agencia);
            entity.ContaCorrente = NullIfEmpty(dados.ContaCorrente);
            entity.NomeEmpresa = NullIfEmpty(dados.NomeEmpresa);
            entity.CodigoEmpresa = NullIfEmpty(dados.CodigoEmpresa);
            entity.NumeroArquivo = dados.NumeroArquivo;
            entity.NomeInicialArquivo = NullIfEmpty(dados.NomeInicialArquivo);
            entity.ExtensaoArquivo = NullIfEmpty(dados.ExtensaoArquivo);
            entity.LayoutArquivo = dados.LayoutArquivo;
            entity.LocalRemessaArquivo = NullIfEmpty(dados.LocalRemessaArquivo);
            entity.LocalRetornoArquivo = NullIfEmpty(dados.LocalRetornoArquivo);
            entity.ComunicaVindi = dados.ComunicaVindi;
            entity.Observacao = NullIfEmpty(dados.Observacao);
            entity.InscricaoEstadual = NullIfEmpty(dados.InscricaoEstadual);
            entity.EstEndereco = NullIfEmpty(dados.EstEndereco);
            entity.EstNumero = NullIfEmpty(dados.EstNumero);
            entity.EstBairro = NullIfEmpty(dados.EstBairro);
            entity.EstComplemento = NullIfEmpty(dados.EstComplemento);
            entity.EstCep = NullIfEmpty(dados.EstCep);
            entity.EstCidade = NullIfEmpty(dados.EstCidade);
            entity.EstUf = NullIfEmpty(dados.EstUf)?.ToUpperInvariant();
            entity.EstNome = NullIfEmpty(dados.EstNome);
            entity.UpdatedAt = DateTime.UtcNow;
            if (!id.HasValue) db.ConvenioCobrancas.Add(entity);
            await db.SaveChangesAsync(token);
            var depois = Mapear(entity, codigo);
            return (depois, entity.PublicId.ToString(), (object?)antes, (object?)depois);
        }, ct);
    public async Task AlterarStatusAsync(Guid id, bool ativo, CancellationToken ct) =>
        await auditoria.ExecutarAsync<bool>(db, "financeiro", "convenio_cobranca", ativo ? "reativar" : "inativar", async token =>
        {
            var e = await db.ConvenioCobrancas.FirstOrDefaultAsync(e => e.PublicId == id, token) ?? throw new ValidacaoException("Convênio não encontrado.");
            var antes = new { e.PublicId, e.Ativo };
            e.Ativo = ativo; e.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(token);
            return (true, e.PublicId.ToString(), (object?)antes, (object?)new { e.PublicId, e.Ativo });
        }, ct);
    private sealed class BancoReferenciaSql
    {
        public long Id { get; set; }
        public string? Codigo { get; set; }
    }
    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static ConvenioCobrancaDto Mapear(ConvenioCobranca e, string? bancoCodigo) => new()
    {
        PublicId = e.PublicId,
        Ativo = e.Ativo,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        BancoCodigo = bancoCodigo,
        Nome = e.Nome ?? string.Empty,
        Agencia = e.Agencia,
        ContaCorrente = e.ContaCorrente,
        NomeEmpresa = e.NomeEmpresa,
        CodigoEmpresa = e.CodigoEmpresa,
        NumeroArquivo = e.NumeroArquivo,
        NomeInicialArquivo = e.NomeInicialArquivo,
        ExtensaoArquivo = e.ExtensaoArquivo,
        LayoutArquivo = e.LayoutArquivo,
        LocalRemessaArquivo = e.LocalRemessaArquivo,
        LocalRetornoArquivo = e.LocalRetornoArquivo,
        ComunicaVindi = e.ComunicaVindi,
        Observacao = e.Observacao,
        InscricaoEstadual = e.InscricaoEstadual,
        EstEndereco = e.EstEndereco,
        EstNumero = e.EstNumero,
        EstBairro = e.EstBairro,
        EstComplemento = e.EstComplemento,
        EstCep = e.EstCep,
        EstCidade = e.EstCidade,
        EstUf = e.EstUf,
        EstNome = e.EstNome
    };
}
