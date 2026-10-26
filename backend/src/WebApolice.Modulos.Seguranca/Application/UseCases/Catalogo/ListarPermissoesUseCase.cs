using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Application.Ports;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Catalogo;

public class ListarPermissoesUseCase
{
    private readonly IConsultasSeguranca _consultas;

    public ListarPermissoesUseCase(IConsultasSeguranca dbContext)
    {
        _consultas = dbContext;
    }

    public Task<IReadOnlyList<CatalogoPermissaoDto>> ExecuteAsync(CancellationToken cancellationToken)
        => _consultas.ListarPermissoesAsync(cancellationToken);
}
