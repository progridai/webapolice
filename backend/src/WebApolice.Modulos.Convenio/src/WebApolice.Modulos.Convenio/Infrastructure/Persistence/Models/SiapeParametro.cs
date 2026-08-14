using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Convenio.src.WebApolice.Modulos.Convenio.Infrastructure.Persistence.Models;

public partial class SiapeParametro
{
    public long Id { get; set; }

    public string? Empresa { get; set; }

    public string? Cgc { get; set; }

    public string? CgcLimpo { get; set; }

    public string? Rubrica { get; set; }

    public string? Comando { get; set; }

    public decimal? CustoLinha { get; set; }

    public string? CalculoParametro { get; set; }

    public int? LegadoId { get; set; }

    public bool Ativo { get; set; }

    public DateTime CreatedAt { get; set; }
}
