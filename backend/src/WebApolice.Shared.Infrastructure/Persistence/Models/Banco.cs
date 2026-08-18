using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class Banco
{
    public long Id { get; set; }

    public string? Codigo { get; set; }

    public string Nome { get; set; } = null!;

    public string? Observacao { get; set; }

    public int? LegadoId { get; set; }

    public DateTime CreatedAt { get; set; }
}
