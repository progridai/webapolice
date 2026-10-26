using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguranca.Contracts;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Authorization;

public sealed class DisponibilidadeCatalogo(SegurancaDbContext db) : IDisponibilidadeCatalogo
{
    public Task<bool> RecursoDisponivelAsync(string modulo, string recurso, CancellationToken cancellationToken) =>
        db.Recursos.AnyAsync(r => r.Codigo == recurso && r.Modulo.Codigo == modulo &&
            r.Ativo && r.Habilitado && r.Modulo.Ativo && r.Modulo.Habilitado, cancellationToken);

    public Task<bool> PermissaoDisponivelAsync(string permissao, CancellationToken cancellationToken) =>
        db.Permissoes.AnyAsync(p => p.Codigo == permissao && p.Ativo &&
            p.Recurso.Ativo && p.Recurso.Habilitado && p.Recurso.Modulo.Ativo && p.Recurso.Modulo.Habilitado,
            cancellationToken);
}
