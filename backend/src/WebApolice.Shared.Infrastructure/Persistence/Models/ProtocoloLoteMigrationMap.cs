using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class ProtocoloLoteMigrationMap
{
    public long Id { get; set; }

    public int LegadoProtocoloId { get; set; }

    public long ProtocoloLoteId { get; set; }

    public int? NumeroProtocoloOriginal { get; set; }

    public DateTime? DataProtocoloOriginal { get; set; }

    public string? CriterioMigracao { get; set; }

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }
}
