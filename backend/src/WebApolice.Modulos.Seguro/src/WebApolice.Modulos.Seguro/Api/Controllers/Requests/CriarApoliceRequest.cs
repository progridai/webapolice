using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.Api.Controllers.Requests;

public class CriarApoliceRequest
{
    public long EstipulanteId { get; set; }
    public long SeguradoraId { get; set; }
    public long? CorretoraId { get; set; }

    public string Nome { get; set; } = null!;
    
    public DateOnly DataInicioVigencia { get; set; }
    public DateOnly? DataFimVigencia { get; set; }
    public DateOnly? DataAniversario { get; set; }

    public List<CriarApoliceRamoRequest>? Ramos { get; set; }
    public List<long>? SubestipulantesIds { get; set; }

    public string? Observacao { get; set; }
}

public class CriarApoliceRamoRequest
{
    public string TipoRamo { get; set; } = null!;
    public string? NumeroApolice { get; set; }
    public decimal? IofPercentual { get; set; }
}
