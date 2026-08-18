using System;
using MediatR;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.AtualizarModulo;

public class AtualizarModuloCommand : IRequest<ModuloDto>
{
    public Guid PublicId { get; set; }
    public string Nome { get; set; } = null!;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; }
}
