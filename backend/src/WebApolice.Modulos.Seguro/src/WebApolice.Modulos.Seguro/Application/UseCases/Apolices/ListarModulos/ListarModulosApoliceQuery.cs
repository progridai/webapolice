using System;
using System.Collections.Generic;
using MediatR;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarModulos;

public class ListarModulosApoliceQuery : IRequest<IEnumerable<ModuloApoliceDto>>
{
    public Guid ApolicePublicId { get; set; }
}
