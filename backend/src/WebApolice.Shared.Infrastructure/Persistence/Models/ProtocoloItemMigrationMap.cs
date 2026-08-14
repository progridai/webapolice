using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class ProtocoloItemMigrationMap
{
    public long Id { get; set; }

    public string OrigemLegado { get; set; } = null!;

    public int LegadoClienteProtocoloId { get; set; }

    public long ProtocoloItemId { get; set; }

    public int? LegadoProtocoloId { get; set; }

    public long? ProtocoloLoteId { get; set; }

    public int? LegadoClienteId { get; set; }

    public long? ClienteId { get; set; }

    public long? ClienteVinculoId { get; set; }

    public long? PessoaId { get; set; }

    public int? LegadoEstipulanteId { get; set; }

    public long? EstipulanteId { get; set; }

    public string? CriterioMigracao { get; set; }

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }
}
