using System;

namespace WebApolice.Modulos.Seguro.Api.Controllers.Requests;

public class AtualizarModuloApoliceRequest
{
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
}
