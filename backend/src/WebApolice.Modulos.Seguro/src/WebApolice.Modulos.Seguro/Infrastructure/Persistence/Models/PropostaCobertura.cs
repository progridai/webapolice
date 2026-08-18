using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public partial class PropostaCobertura
{
    public long Id { get; set; }

    public long PropostaId { get; set; }

    public long? PropostaItemId { get; set; }

    public long? CoberturaId { get; set; }

    public decimal? PremioTitular { get; set; }

    public decimal? PremioConjuge { get; set; }

    public bool? Basica { get; set; }

    public string? CoberturaNomeLegado { get; set; }

    public int LegadoId { get; set; }

    public int? LegadoPropostaCoberturaAnt { get; set; }

    public bool Ativo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Cobertura? Cobertura { get; set; }

    public virtual Propostum Proposta { get; set; } = null!;

    public virtual PropostaItem? PropostaItem { get; set; }
}
