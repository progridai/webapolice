using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Domain;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Persistence.Repositories;

internal sealed class UsuarioProvisionamentoRepository : IUsuarioProvisionamentoRepository
{
    private readonly SegurancaDbContext _dbContext;

    public UsuarioProvisionamentoRepository(SegurancaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Usuario?> ObterPorKeycloakSubParaAtualizacaoAsync(string keycloakSub, CancellationToken cancellationToken)
    {
        // Usa tracking padrão (sem AsNoTracking) para permitir atualização de propriedades
        return await _dbContext.Usuarios
            .SingleOrDefaultAsync(u => u.KeycloakSub == keycloakSub, cancellationToken);
    }

    public async Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        await _dbContext.Usuarios.AddAsync(usuario, cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void LimparRastreamento()
    {
        _dbContext.ChangeTracker.Clear();
    }
}
