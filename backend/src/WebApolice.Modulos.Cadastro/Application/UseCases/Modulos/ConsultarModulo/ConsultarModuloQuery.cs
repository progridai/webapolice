using System;
using MediatR;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.ConsultarModulo;

public class ConsultarModuloQuery : IRequest<ModuloDto>
{
    public Guid PublicId { get; set; }
}
