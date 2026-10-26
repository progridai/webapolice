using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Application.Ports;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Usuarios;

public class ListarUsuariosUseCase
{
    private readonly IConsultasSeguranca _consultas;

    public ListarUsuariosUseCase(IConsultasSeguranca dbContext)
    {
        _consultas = dbContext;
    }

    public Task<ListagemPaginadaDto<UsuarioListDto>> ExecuteAsync(
        string? busca,
        bool? ativo,
        Guid? perfilPublicId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken)
        => _consultas.ListarUsuariosAsync(busca, ativo, perfilPublicId, pagina, tamanhoPagina, cancellationToken);
}
