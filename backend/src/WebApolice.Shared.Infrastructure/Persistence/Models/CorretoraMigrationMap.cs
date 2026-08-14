using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class CorretoraMigrationMap
{
    public long Id { get; set; }

    public int LegadoCorretoraId { get; set; }

    public long CorretoraId { get; set; }

    public long? PessoaId { get; set; }

    public string? NomeOriginal { get; set; }

    public string? CriterioMigracao { get; set; }

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }
}
