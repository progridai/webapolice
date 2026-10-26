using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguranca.Domain;
using WebApolice.Modulos.Seguranca.Domain.Auditoria;
using WebApolice.SharedKernel.Application.Persistence;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Shared.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Persistence.Repositories;

public sealed class SegurancaAdministracaoRepository(SegurancaDbContext db) : ISegurancaAdministracaoRepository
{
    public Task<List<Perfil>> ObterPerfisAsync(List<Guid> publicIds, bool somenteAtivos, CancellationToken ct) => db.Perfis.Where(p => publicIds.Contains(p.PublicId) && (!somenteAtivos || p.Ativo)).ToListAsync(ct);
    public Task<List<Permissao>> ObterPermissoesAsync(List<Guid> publicIds, bool somenteAtivas, CancellationToken ct) => db.Permissoes.Where(p => publicIds.Contains(p.PublicId) && (!somenteAtivas || p.Ativo)).ToListAsync(ct);
    public Task<Usuario?> ObterUsuarioPorSubAsync(string? sub, CancellationToken ct) => db.Usuarios.FirstOrDefaultAsync(u => u.KeycloakSub == sub, ct);
    public Task<Usuario?> ObterUsuarioComPerfisAsync(Guid publicId, CancellationToken ct) => db.Usuarios.Include(u => u.Perfis).ThenInclude(p => p.Perfil).FirstOrDefaultAsync(u => u.PublicId == publicId, ct);
    public Task<Perfil?> ObterPerfilComPermissoesAsync(Guid publicId, CancellationToken ct) => db.Perfis.Include(p => p.Permissoes).ThenInclude(p => p.Permissao).FirstOrDefaultAsync(p => p.PublicId == publicId, ct);
    public Task<Modulo?> ObterModuloAsync(Guid publicId, CancellationToken ct) => db.Modulos.FirstOrDefaultAsync(m => m.PublicId == publicId, ct);
    public Task<Recurso?> ObterRecursoAsync(Guid publicId, CancellationToken ct) => db.Recursos.Include(r => r.Modulo).FirstOrDefaultAsync(r => r.PublicId == publicId, ct);
    public Task<bool> UsuarioDuplicadoAsync(string username, string email, CancellationToken ct) => db.Usuarios.AnyAsync(u => u.Username == username || u.Email == email, ct);
    public Task<bool> CodigoPerfilExisteAsync(string codigo, CancellationToken ct) => db.Perfis.AnyAsync(p => p.Codigo == codigo, ct);
    public void AdicionarUsuario(Usuario usuario) => db.Usuarios.Add(usuario);
    public void AdicionarPerfil(Perfil perfil) => db.Perfis.Add(perfil);
    public void AdicionarAuditoria(AuditoriaPermissao evento) => db.AuditoriaPermissoes.Add(evento);
    public async Task SalvarAlteracoesAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
    public async Task<ITransacaoPersistencia> BeginTransactionAsync(CancellationToken ct) => new TransacaoEf(await db.Database.BeginTransactionAsync(ct));
}
