using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class TabelaPrecoMigrationMap
{
    public long Id { get; set; }

    public int LegadoTabelaId { get; set; }

    public long TabelaPrecoId { get; set; }

    public string? NomeOriginal { get; set; }

    public DateTime CreatedAt { get; set; }
}
