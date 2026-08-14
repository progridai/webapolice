using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class PropostaCoberturaMigrationMap
{
    public long Id { get; set; }

    public int LegadoPropostaCoberturaId { get; set; }

    public long PropostaCoberturaId { get; set; }

    public int? LegadoPropostaId { get; set; }

    public long? PropostaId { get; set; }

    public int? LegadoPropostaTipoId { get; set; }

    public long? PropostaItemId { get; set; }

    public int? LegadoCoberturaId { get; set; }

    public long? CoberturaId { get; set; }

    public string? CriterioMigracao { get; set; }

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }
}
