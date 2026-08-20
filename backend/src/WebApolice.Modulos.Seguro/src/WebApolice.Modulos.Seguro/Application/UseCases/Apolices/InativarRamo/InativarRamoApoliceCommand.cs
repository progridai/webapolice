using System;
using MediatR;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarRamo;

public class InativarRamoApoliceCommand : IRequest<bool>
{
    public Guid ApolicePublicId { get; set; }
    public Guid RamoPublicId { get; set; }
    public Guid UsuarioPublicId { get; set; }
}
