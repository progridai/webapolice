using System;

namespace WebApolice.Modulos.Seguro.Api.Controllers.Requests;

public class VincularSubestipulanteApoliceRequest
{
    public Guid SubestipulantePublicId { get; set; }
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
}
