using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;
using WebApolice.Auditoria.Infrastructure.Mascaramento;

namespace WebApolice.Auditoria.Infrastructure;

public class RegistradorAuditoria : IRegistradorAuditoria
{
    private readonly AuditoriaDbContext _dbContext;

    public RegistradorAuditoria(AuditoriaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RegistrarAsync(RegistroAuditoria registro, CancellationToken cancellationToken = default)
    {
        // Valida e rejeita se houver campos sensíveis
        registro.DadosAnteriores = ProvedorMascaramento.ValidarERejeitarSegredos(registro.DadosAnteriores);
        registro.DadosPosteriores = ProvedorMascaramento.ValidarERejeitarSegredos(registro.DadosPosteriores);
        registro.Metadados = ProvedorMascaramento.ValidarERejeitarSegredos(registro.Metadados);

        _dbContext.RegistrosAuditoria.Add(registro);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
