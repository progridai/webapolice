using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public partial class TabelaPreco
{
    public long Id { get; set; }

    public string? Nome { get; set; }

    public string? Codigo { get; set; }

    public int? LegadoId { get; set; }

    public bool Ativo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Produto> Produtos { get; set; } = new List<Produto>();

    public virtual ICollection<PropostaItem> PropostaItems { get; set; } = new List<PropostaItem>();
}
