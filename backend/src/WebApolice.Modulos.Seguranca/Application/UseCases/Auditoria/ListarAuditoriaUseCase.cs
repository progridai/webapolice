using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Application.Ports;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Auditoria;

public class ListarAuditoriaUseCase
{
    private readonly IConsultasSeguranca _consultas;

    public ListarAuditoriaUseCase(IConsultasSeguranca dbContext)
    {
        _consultas = dbContext;
    }

    public Task<ListagemPaginadaDto<AuditoriaDto>> ExecuteAsync(
        int pagina,
        int tamanhoPagina,
        string? acao,
        string? entidade,
        DateTime? dataInicial,
        DateTime? dataFinal,
        CancellationToken cancellationToken)
        => _consultas.ListarAuditoriaAsync(pagina, tamanhoPagina, acao, entidade, dataInicial, dataFinal, cancellationToken);
}
