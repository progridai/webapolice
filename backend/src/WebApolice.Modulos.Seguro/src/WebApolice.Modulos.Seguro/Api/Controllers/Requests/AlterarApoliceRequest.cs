using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.Api.Controllers.Requests;

public class AlterarApoliceRequest
{
    public long EstipulanteId { get; set; }
    public long SeguradoraId { get; set; }
    public long? CorretoraId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateOnly DataInicioVigencia { get; set; }
    public DateOnly? DataFimVigencia { get; set; }
    public DateOnly? DataAniversario { get; set; }
    public string? Observacao { get; set; }
}
