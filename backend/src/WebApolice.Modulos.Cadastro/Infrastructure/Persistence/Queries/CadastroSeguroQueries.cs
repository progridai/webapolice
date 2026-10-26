using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Contracts;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;

public sealed class CadastroSeguroQueries(CadastroDbContext db) : ICadastroSeguroQueries
{
    public Task<ClienteSeguro?> ObterClienteAsync(Guid publicId, CancellationToken ct) =>
        (from c in db.Clientes.AsNoTracking() join p in db.Pessoas.AsNoTracking() on c.PessoaId equals p.Id
         where c.PublicId == publicId && c.DeletedAt == null
         select new ClienteSeguro(c.Id, c.PublicId, p.Nome, p.DocumentoPrincipal)).FirstOrDefaultAsync(ct);
    public Task<ModuloSeguro?> ObterModuloAsync(Guid publicId, CancellationToken ct) => db.Modulos.AsNoTracking()
        .Where(m => m.PublicId == publicId && m.DeletedAt == null)
        .Select(m => new ModuloSeguro(m.Id, m.PublicId, m.Nome, m.Descricao, m.Ativo)).FirstOrDefaultAsync(ct);
    // A leitura por ID preserva refer?ncias hist?ricas, como as consultas anteriores.
    public Task<ModuloSeguro?> ObterModuloPorIdAsync(long id, CancellationToken ct) => db.Modulos.AsNoTracking()
        .Where(m => m.Id == id).Select(m => new ModuloSeguro(m.Id, m.PublicId, m.Nome, m.Descricao, m.Ativo)).FirstOrDefaultAsync(ct);
    public async Task<IReadOnlyList<ModuloSeguro>> ObterModulosAsync(IReadOnlyCollection<long> ids, CancellationToken ct) => await db.Modulos.AsNoTracking()
        .Where(m => ids.Contains(m.Id)).Select(m => new ModuloSeguro(m.Id, m.PublicId, m.Nome, m.Descricao, m.Ativo)).ToListAsync(ct);
    public Task<SubestipulanteSeguro?> ObterSubestipulanteAsync(Guid publicId, CancellationToken ct) => db.Subestipulantes.AsNoTracking()
        .Where(s => s.PublicId == publicId).Select(s => new SubestipulanteSeguro(s.Id, s.Ativo)).FirstOrDefaultAsync(ct);
}
