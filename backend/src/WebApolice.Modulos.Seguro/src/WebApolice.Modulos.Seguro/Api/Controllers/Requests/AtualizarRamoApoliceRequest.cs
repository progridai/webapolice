using System;

namespace WebApolice.Modulos.Seguro.Api.Controllers.Requests;

public class AtualizarRamoApoliceRequest
{
    public string? NumeroApolice { get; set; }
    public decimal? IofPercentual { get; set; }
}
