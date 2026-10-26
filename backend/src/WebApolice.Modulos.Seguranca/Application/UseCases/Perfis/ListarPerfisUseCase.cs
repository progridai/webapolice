using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Application.Ports;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Perfis;

public class ListarPerfisUseCase
{
    private readonly IConsultasSeguranca _consultas;

    public ListarPerfisUseCase(IConsultasSeguranca dbContext)
    {
        _consultas = dbContext;
    }

    public Task<ListagemPaginadaDto<PerfilDto>> ExecuteAsync(
        string? busca,
        bool? ativo,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken)
        => _consultas.ListarPerfisAsync(busca, ativo, pagina, tamanhoPagina, cancellationToken);
}
