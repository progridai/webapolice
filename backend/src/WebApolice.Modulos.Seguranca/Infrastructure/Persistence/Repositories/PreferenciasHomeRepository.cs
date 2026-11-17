using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Domain;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Persistence.Repositories;

public sealed class PreferenciasHomeRepository(SegurancaDbContext db) : IPreferenciasHomeRepository
{
    public Task<Usuario?> ObterUsuarioAtivoAsync(string keycloakSub, CancellationToken cancellationToken) =>
        db.Usuarios.SingleOrDefaultAsync(u => u.KeycloakSub == keycloakSub && u.Ativo, cancellationToken);

    public Task SalvarAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
