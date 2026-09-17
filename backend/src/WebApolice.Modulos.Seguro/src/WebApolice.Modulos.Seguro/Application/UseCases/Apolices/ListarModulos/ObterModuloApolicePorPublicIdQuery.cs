using System;
using MediatR;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarModulos;

public class ObterModuloApolicePorPublicIdQuery : IRequest<ModuloApoliceDto>
{
    public Guid ApolicePublicId { get; set; }
    public Guid ApoliceModuloPublicId { get; set; }
}
