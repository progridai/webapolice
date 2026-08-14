using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class SinistroMigrationMap
{
    public long Id { get; set; }

    public int LegadoSinistroId { get; set; }

    public long SinistroId { get; set; }

    public int? LegadoPropostaId { get; set; }

    public long? PropostaId { get; set; }

    public long? PessoaId { get; set; }

    public long? ClienteId { get; set; }

    public long? ClienteVinculoId { get; set; }

    public int? LegadoStatus { get; set; }

    public short? StatusId { get; set; }

    public string? NumeroSinistroOriginal { get; set; }

    public string? CriterioMigracao { get; set; }

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }
}
