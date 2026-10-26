using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Application.Ports;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Perfis;

public class ObterPerfilUseCase
{
    private readonly IConsultasSeguranca _consultas;

    public ObterPerfilUseCase(IConsultasSeguranca dbContext)
    {
        _consultas = dbContext;
    }

    public Task<PerfilDetalheDto?> ExecuteAsync(Guid publicId, CancellationToken cancellationToken)
        => _consultas.ObterPerfilAsync(publicId, cancellationToken);
}
