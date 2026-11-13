using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Application.UseCases.Vinculos;
using WebApolice.Modulos.Cadastro.Domain;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;
using WebApolice.Modulos.Cadastro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Models;
namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Repositories;

public sealed class ClienteVinculosRepository(CadastroDbContext db) : IClienteVinculosRepository
{
    public Task<Cliente?> ObterClienteAsync(Guid id, bool bloquear, CancellationToken ct) =>
        (bloquear && db.Database.IsNpgsql()
            ? db.Clientes.FromSqlInterpolated($"SELECT * FROM cadastro.cliente WHERE public_id = {id} FOR UPDATE")
            : db.Clientes.Where(c => c.PublicId == id))
        .FirstOrDefaultAsync(c => c.DeletedAt == null, ct);
    public Task<ClienteVinculoModel?> ObterAsync(Guid clienteId, Guid id, CancellationToken ct) =>
        (from v in db.Vinculos
         join c in db.Clientes on v.ClienteId equals c.Id
         where c.PublicId == clienteId && v.PublicId == id
         select v).SingleOrDefaultAsync(ct);
    public Task<EstipulanteModel?> ObterEstipulanteAsync(Guid id, CancellationToken ct) => db.Estipulantes.FirstOrDefaultAsync(e => e.PublicId == id, ct);
    public Task<bool> DuplicadoAsync(long clienteId, long estipulanteId, string matricula, long? excetoId, CancellationToken ct) =>
        db.Vinculos.AnyAsync(v => v.ClienteId == clienteId && v.EstipulanteId == estipulanteId && v.Ativo &&
            (!excetoId.HasValue || v.Id != excetoId) && (v.MatriculaNormalizada ?? (v.Matricula ?? "").Trim().ToUpper()) == matricula, ct);
    public async Task<PagedResult<ClienteVinculoConsulta>> ListarAsync(Guid clienteId, int pagina, int tamanho, CancellationToken ct)
    {
        var query = from v in db.Vinculos.AsNoTracking()
                    join c in db.Clientes on v.ClienteId equals c.Id
                    where c.PublicId == clienteId
                    select v;
        var total = await query.CountAsync(ct);
        var items = await (from v in query.OrderByDescending(v => v.Ativo).ThenBy(v => v.Id).Skip((pagina - 1) * tamanho).Take(tamanho)
                           join e in db.Estipulantes on v.EstipulanteId equals e.Id into es
                           from e in es.DefaultIfEmpty()
                           join g in db.Grupos on v.GrupoId equals g.Id into gs
                           from g in gs.DefaultIfEmpty()
                           join s in db.Subgrupos on v.SubgrupoId equals s.Id into ss
                           from s in ss.DefaultIfEmpty()
                           join l in db.Lotacoes on v.LotacaoId equals l.Id into ls
                           from l in ls.DefaultIfEmpty()
                           select new ClienteVinculoConsulta(v.Id, new ClienteVinculoResult(v.PublicId, v.Matricula ?? "", v.Ativo,
                               e == null ? null : e.PublicId, e == null ? null : e.Nome, g == null ? null : g.Nome,
                               s == null ? null : s.Nome, l == null ? null : l.Nome))).ToListAsync(ct);
        return new PagedResult<ClienteVinculoConsulta> { Items = items, Page = pagina, PageSize = tamanho, TotalCount = total };
    }
    public async Task<PagedResult<EstipulanteVinculoOpcao>> ListarEstipulantesAsync(string? busca, int pagina, int tamanho, CancellationToken ct)
    {
        var query = db.Estipulantes.AsNoTracking().Where(e => e.Ativo && e.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().ToUpperInvariant();
            query = query.Where(e => e.Nome.ToUpper().Contains(termo));
        }
        return new()
        {
            Items = await query.OrderBy(e => e.Nome).ThenBy(e => e.Id).Skip((pagina - 1) * tamanho).Take(tamanho)
            .Select(e => new EstipulanteVinculoOpcao(e.PublicId, e.Nome)).ToListAsync(ct),
            TotalCount = await query.CountAsync(ct),
            Page = pagina,
            PageSize = tamanho
        };
    }
    public void Adicionar(ClienteVinculoModel vinculo) => db.Vinculos.Add(vinculo);
    public async Task SalvarAsync(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { ConstraintName: "ck_cliente_vinculo_identidade_em_uso" })
        { throw new ClienteInvalidoException("Este vínculo já está em uso. Cadastre outro vínculo para mudar matrícula ou estipulante."); }
    }
}
