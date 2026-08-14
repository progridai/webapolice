using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class CoberturaMigrationMap
{
    public long Id { get; set; }

    public int LegadoCoberturaId { get; set; }

    public long CoberturaId { get; set; }

    public string? NomeOriginal { get; set; }

    public DateTime CreatedAt { get; set; }
}
