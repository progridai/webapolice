using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Application.Ports;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Auditoria;

public class ObterAuditoriaUseCase
{
    private readonly IConsultasSeguranca _consultas;

    public ObterAuditoriaUseCase(IConsultasSeguranca dbContext)
    {
        _consultas = dbContext;
    }

    public Task<AuditoriaDto?> ExecuteAsync(Guid publicId, CancellationToken cancellationToken)
        => _consultas.ObterAuditoriaAsync(publicId, cancellationToken);
}
