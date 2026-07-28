using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Application.Services;

public class AcessoOperadorSistemaService : IAcessoOperadorSistemaService
{
    private readonly SegurancaDbContext _dbContext;

    public AcessoOperadorSistemaService(SegurancaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> EhOperadorSistemaAsync(string keycloakSub, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keycloakSub))
            return false;

        return await _dbContext.Usuarios
            .Where(u => u.Ativo && u.KeycloakSub == keycloakSub)
            .SelectMany(u => u.Perfis)
            .AnyAsync(up => up.Perfil.Ativo && up.Perfil.Codigo == "ADMINISTRADOR" && up.Perfil.PerfilSistema, cancellationToken);
    }
}
