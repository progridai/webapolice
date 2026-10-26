using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Application.Ports;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Usuarios;

public class ObterUsuarioUseCase
{
    private readonly IConsultasSeguranca _consultas;

    public ObterUsuarioUseCase(IConsultasSeguranca dbContext)
    {
        _consultas = dbContext;
    }

    public Task<UsuarioDetalheDto?> ExecuteAsync(Guid publicId, CancellationToken cancellationToken)
        => _consultas.ObterUsuarioAsync(publicId, cancellationToken);
}
