using System;
using MediatR;
using WebApolice.SharedKernel.Application.Models;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.ListarModulos;

public class ListarModulosQuery : IRequest<PagedResult<ModuloListDto>>
{
    public string? Busca { get; set; }
    public bool? Ativo { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 10;
}
