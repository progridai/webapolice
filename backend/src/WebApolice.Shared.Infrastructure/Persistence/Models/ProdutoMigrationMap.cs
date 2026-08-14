using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class ProdutoMigrationMap
{
    public long Id { get; set; }

    public int LegadoProdutoId { get; set; }

    public long ProdutoId { get; set; }

    public string? CodigoReferenciaOriginal { get; set; }

    public DateTime CreatedAt { get; set; }
}
