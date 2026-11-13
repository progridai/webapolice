using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Contracts;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;

public sealed class CadastroSeguroQueries(CadastroDbContext db, IClienteVinculosUso uso) : ICadastroSeguroQueries
{
    private IQueryable<VinculoClienteSeguro> Vinculos =>
        from v in db.Vinculos.AsNoTracking()
        join c in db.Clientes on v.ClienteId equals c.Id
        join p in db.Pessoas on c.PessoaId equals p.Id
        join e in db.Estipulantes on v.EstipulanteId equals e.Id into es
        from e in es.DefaultIfEmpty()
        where c.DeletedAt == null && p.DeletedAt == null && v.PessoaId == p.Id
        select new VinculoClienteSeguro
        {
            Id = v.Id,
            PublicId = v.PublicId,
            ClienteId = c.Id,
            PessoaId = p.Id,
            EstipulanteId = v.EstipulanteId,
            Matricula = v.Matricula ?? "",
            Ativo = v.Ativo,
            Estipulante = e == null ? null : e.Nome,
            EstipulanteAtivo = e != null && e.Ativo && e.DeletedAt == null
        };
    public Task<VinculoClienteSeguro?> ObterVinculoAsync(Guid publicId, long clienteId, CancellationToken ct) =>
        Vinculos.SingleOrDefaultAsync(v => v.PublicId == publicId && v.ClienteId == clienteId, ct);
    public async Task<IReadOnlyList<VinculoClienteSeguro>> ObterVinculosAsync(IReadOnlyCollection<long> ids, CancellationToken ct) =>
        await Vinculos.Where(v => ids.Contains(v.Id)).ToListAsync(ct);
    public async Task<WebApolice.SharedKernel.Application.Models.PagedResult<VinculoClienteSeguro>> ListarVinculosAsync(Guid clienteId, long estipulanteId, int pagina, int tamanho, CancellationToken ct)
    {
        var cliente = await db.Clientes.Where(c => c.PublicId == clienteId && c.DeletedAt == null).Select(c => (long?)c.Id).SingleOrDefaultAsync(ct);
        if (!await db.Estipulantes.AnyAsync(e => e.Id == estipulanteId && e.Ativo && e.DeletedAt == null, ct))
            return new() { Items = [], Page = pagina, PageSize = tamanho, TotalCount = 0 };
        var query = Vinculos.Where(v => v.ClienteId == cliente && v.Ativo && v.Matricula.Trim() != "" &&
            ((v.EstipulanteId == estipulanteId && v.EstipulanteAtivo) || (v.EstipulanteId == null && v.Matricula.Length <= 50)));
        var candidatos = await query.Where(v => v.EstipulanteId == null).Select(v => v.Id).ToListAsync(ct);
        var emUso = await uso.ListarEmUsoAsync(candidatos, ct);
        query = query.Where(v => v.EstipulanteId != null || !emUso.Contains(v.Id));
        return new()
        {
            Items = await query.OrderBy(v => v.Matricula).ThenBy(v => v.Id).Skip((pagina - 1) * tamanho).Take(tamanho).ToListAsync(ct),
            TotalCount = await query.CountAsync(ct),
            Page = pagina,
            PageSize = tamanho
        };
    }
    public Task<ClienteSeguro?> ObterClienteAsync(Guid publicId, CancellationToken ct) =>
        (from c in db.Clientes.AsNoTracking()
         join p in db.Pessoas.AsNoTracking() on c.PessoaId equals p.Id
         where c.PublicId == publicId && c.DeletedAt == null
         select new ClienteSeguro(c.Id, c.PublicId, p.Nome, p.DocumentoPrincipal)).FirstOrDefaultAsync(ct);
    public Task<ModuloSeguro?> ObterModuloAsync(Guid publicId, CancellationToken ct) => db.Modulos.AsNoTracking()
        .Where(m => m.PublicId == publicId && m.DeletedAt == null)
        .Select(m => new ModuloSeguro(m.Id, m.PublicId, m.Nome, m.Descricao, m.Ativo)).FirstOrDefaultAsync(ct);
    // A leitura por ID preserva refer?ncias hist?ricas, como as consultas anteriores.
    public Task<ModuloSeguro?> ObterModuloPorIdAsync(long id, CancellationToken ct) => db.Modulos.AsNoTracking()
        .Where(m => m.Id == id).Select(m => new ModuloSeguro(m.Id, m.PublicId, m.Nome, m.Descricao, m.Ativo)).FirstOrDefaultAsync(ct);
    public async Task<ModuloSeguro?> ObterModuloParaConfirmacaoAsync(long id, CancellationToken ct)
    {
        if (db.Database.IsRelational())
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM cadastro.modulo WHERE id={id} FOR SHARE", ct);
        return await db.Modulos.AsNoTracking().Where(m => m.Id == id && m.DeletedAt == null)
            .Select(m => new ModuloSeguro(m.Id, m.PublicId, m.Nome, m.Descricao, m.Ativo)).SingleOrDefaultAsync(ct);
    }
    public async Task<IReadOnlyList<ModuloSeguro>> ObterModulosAsync(IReadOnlyCollection<long> ids, CancellationToken ct) => await db.Modulos.AsNoTracking()
        .Where(m => ids.Contains(m.Id)).Select(m => new ModuloSeguro(m.Id, m.PublicId, m.Nome, m.Descricao, m.Ativo)).ToListAsync(ct);
    public Task<SubestipulanteSeguro?> ObterSubestipulanteAsync(Guid publicId, CancellationToken ct) => db.Subestipulantes.AsNoTracking()
        .Where(s => s.PublicId == publicId).Select(s => new SubestipulanteSeguro(s.Id, s.Ativo)).FirstOrDefaultAsync(ct);
}
