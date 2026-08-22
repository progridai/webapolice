using System;

namespace WebApolice.Modulos.Seguro.Api.Controllers.Requests;

public class AtualizarSubestipulanteApoliceRequest
{
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
}
