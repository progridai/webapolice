using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class SinistroAcompanhamentoMigrationMap
{
    public long Id { get; set; }

    public int LegadoAcompanhamentoId { get; set; }

    public long AcompanhamentoId { get; set; }

    public int? LegadoSinistroId { get; set; }

    public long? SinistroId { get; set; }

    public string? CriterioMigracao { get; set; }

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }
}
