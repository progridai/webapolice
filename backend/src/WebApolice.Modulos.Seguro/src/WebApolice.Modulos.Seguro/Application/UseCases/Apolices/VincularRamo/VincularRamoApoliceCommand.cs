using System;
using MediatR;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.VincularRamo;

public class VincularRamoApoliceCommand : IRequest<long>
{
    public Guid ApolicePublicId { get; set; }
    public Guid RamoPublicId { get; set; }
    public string? NumeroApolice { get; set; }
    public decimal? IofPercentual { get; set; }
    public Guid UsuarioPublicId { get; set; }
}
