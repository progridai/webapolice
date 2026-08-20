using System;

namespace WebApolice.Modulos.Seguro.Api.Controllers.Requests;

public class VincularRamoApoliceRequest
{
    public Guid RamoPublicId { get; set; }
    public string? NumeroApolice { get; set; }
    public decimal? IofPercentual { get; set; }
}
