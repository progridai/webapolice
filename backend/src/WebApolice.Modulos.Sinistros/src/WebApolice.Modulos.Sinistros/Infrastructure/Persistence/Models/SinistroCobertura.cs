using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Sinistros.src.WebApolice.Modulos.Sinistros.Infrastructure.Persistence.Models;

public partial class SinistroCobertura
{
    public long Id { get; set; }

    public long SinistroId { get; set; }

    public long? PropostaId { get; set; }

    public long? PropostaCoberturaId { get; set; }

    public long? CoberturaId { get; set; }

    public decimal? ValorEstimado { get; set; }

    public decimal? ValorPago { get; set; }

    public string? Observacao { get; set; }

    public int? LegadoId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CoberturaSinistroLegadoId { get; set; }

    public decimal? PremioTitular { get; set; }

    public decimal? PremioConjuge { get; set; }

    public virtual Sinistro Sinistro { get; set; } = null!;
}
