using System;
using MediatR;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.CriarModulo;

public class CriarModuloCommand : IRequest<ModuloDto>
{
    public string Nome { get; set; } = null!;
    public string? Descricao { get; set; }
}
