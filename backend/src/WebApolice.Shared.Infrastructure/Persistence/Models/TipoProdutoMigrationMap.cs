using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class TipoProdutoMigrationMap
{
    public long Id { get; set; }

    public int LegadoTipoId { get; set; }

    public long TipoProdutoId { get; set; }

    public string? NomeOriginal { get; set; }

    public DateTime CreatedAt { get; set; }
}
