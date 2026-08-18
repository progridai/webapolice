using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public partial class TipoProduto
{
    public long Id { get; set; }

    public string Nome { get; set; } = null!;

    public int? LegadoId { get; set; }

    public bool Ativo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<PropostaItem> PropostaItems { get; set; } = new List<PropostaItem>();
}
