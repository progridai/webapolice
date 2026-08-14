using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class PropostaMigrationMap
{
    public long Id { get; set; }

    public int LegadoPropostaId { get; set; }

    public long PropostaId { get; set; }

    public int? LegadoClienteId { get; set; }

    public long? ClienteId { get; set; }

    public long? ClienteVinculoId { get; set; }

    public long? PessoaId { get; set; }

    public int? LegadoEstipulanteId { get; set; }

    public long? EstipulanteId { get; set; }

    public int? LegadoSubestipulanteId { get; set; }

    public long? SubestipulanteId { get; set; }

    public int? LegadoStatus { get; set; }

    public short? StatusId { get; set; }

    public string? NumeroOriginal { get; set; }

    public string? CriterioMigracao { get; set; }

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }
}
