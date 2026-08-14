using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class PropostaParticipanteMigrationMap
{
    public long Id { get; set; }

    public long PropostaParticipanteId { get; set; }

    public int? LegadoPropostaId { get; set; }

    public long? PropostaId { get; set; }

    public string ParticipanteTipo { get; set; } = null!;

    public int? CodigoLegadoParticipante { get; set; }

    public long? AgenciadorId { get; set; }

    public long? CorretoraId { get; set; }

    public string? CampoOrigem { get; set; }

    public string? CriterioMigracao { get; set; }

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }
}
