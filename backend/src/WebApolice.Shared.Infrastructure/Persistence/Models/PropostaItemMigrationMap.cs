using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class PropostaItemMigrationMap
{
    public long Id { get; set; }

    public int LegadoPropostaTipoId { get; set; }

    public long PropostaItemId { get; set; }

    public int? LegadoPropostaId { get; set; }

    public long? PropostaId { get; set; }

    public int? LegadoTipoId { get; set; }

    public long? TipoProdutoId { get; set; }

    public int? LegadoProdutoId { get; set; }

    public long? ProdutoId { get; set; }

    public string? LegadoPlanoOriginal { get; set; }

    public long? PlanoId { get; set; }

    public int? LegadoTabelaId { get; set; }

    public long? TabelaPrecoId { get; set; }

    public string? CriterioMigracao { get; set; }

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }
}
