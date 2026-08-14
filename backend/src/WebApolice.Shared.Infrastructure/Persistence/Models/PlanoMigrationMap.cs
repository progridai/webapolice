using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class PlanoMigrationMap
{
    public long Id { get; set; }

    public int LegadoPlanoId { get; set; }

    public long PlanoId { get; set; }

    public string? NomeOriginal { get; set; }

    public DateTime CreatedAt { get; set; }
}
