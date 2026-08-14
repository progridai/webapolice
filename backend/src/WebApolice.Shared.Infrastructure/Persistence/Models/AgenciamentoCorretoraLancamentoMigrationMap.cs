using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class AgenciamentoCorretoraLancamentoMigrationMap
{
    public long Id { get; set; }

    public int LegadoAgenciamentoId { get; set; }

    public long AgenciamentoCorretoraLancamentoId { get; set; }

    public int? LegadoPropostaId { get; set; }

    public long? PropostaId { get; set; }

    public int? LegadoCorretoraId { get; set; }

    public long? CorretoraId { get; set; }

    public int? LegadoMovimentoId { get; set; }

    public long? MovimentoTipoId { get; set; }

    public string? CriterioMigracao { get; set; }

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }
}
