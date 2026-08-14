using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public partial class Plano
{
    public long Id { get; set; }

    public string? Nome { get; set; }

    public string? Ramo { get; set; }

    public bool? Paga { get; set; }

    public bool? Reajuste { get; set; }

    public int? LegadoId { get; set; }

    public int? LegadoPlanoAnt { get; set; }

    public bool Ativo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Produto> Produtos { get; set; } = new List<Produto>();

    public virtual ICollection<PropostaItem> PropostaItems { get; set; } = new List<PropostaItem>();
}
