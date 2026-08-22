using System;

namespace WebApolice.Modulos.Seguro.Api.Controllers.Requests;

public class VincularModuloApoliceRequest
{
    public Guid ModuloPublicId { get; set; }
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
}
