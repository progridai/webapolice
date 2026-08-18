using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence.Models;

public partial class FormaRetorno
{
    public long Id { get; set; }

    public string Nome { get; set; } = null!;

    public int LegadoId { get; set; }

    public bool Ativo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<FormaRetornoEstipulante> FormaRetornoEstipulantes { get; set; } = new List<FormaRetornoEstipulante>();
}
