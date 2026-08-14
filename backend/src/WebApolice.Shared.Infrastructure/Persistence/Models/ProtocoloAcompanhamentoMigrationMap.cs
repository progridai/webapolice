using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class ProtocoloAcompanhamentoMigrationMap
{
    public long Id { get; set; }

    public int LegadoAcompanhamentoId { get; set; }

    public long ProtocoloAcompanhamentoId { get; set; }

    public int? LegadoProtocoloId { get; set; }

    public long? ProtocoloLoteId { get; set; }

    public string? CriterioMigracao { get; set; }

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }
}
