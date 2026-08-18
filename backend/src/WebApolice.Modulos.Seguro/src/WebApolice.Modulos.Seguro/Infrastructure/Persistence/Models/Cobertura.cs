using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public partial class Cobertura
{
    public long Id { get; set; }

    public string? Nome { get; set; }

    public string? NomeReduzido { get; set; }

    public string? Basica { get; set; }

    public bool? Reajuste { get; set; }

    public int? LegadoId { get; set; }

    public int? LegadoCoberturaAnt { get; set; }

    public bool Ativo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<PropostaCobertura> PropostaCoberturas { get; set; } = new List<PropostaCobertura>();
}
