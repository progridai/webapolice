using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence.Models;

public partial class FormaRetornoEstipulante
{
    public long Id { get; set; }

    public long FormaRetornoId { get; set; }

    public long? EstipulanteId { get; set; }

    public int LegadoId { get; set; }

    public int? LegadoFormaRetornoId { get; set; }

    public int? LegadoEstipulanteId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual FormaRetorno FormaRetorno { get; set; } = null!;
}
