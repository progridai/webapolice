using System;
using MediatR;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.InativarModulo;

public class InativarModuloCommand : IRequest<bool>
{
    public Guid PublicId { get; set; }
}
