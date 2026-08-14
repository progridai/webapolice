using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class AgenciadorMigrationMap
{
    public long Id { get; set; }

    public int LegadoAgenciadorId { get; set; }

    public long AgenciadorId { get; set; }

    public long? PessoaId { get; set; }

    public string? NomeOriginal { get; set; }

    public string? CpfOriginal { get; set; }

    public string? CpfLimpo { get; set; }

    public bool CpfValido { get; set; }

    public int? LegadoCoordenadorId { get; set; }

    public long? CoordenadorId { get; set; }

    public string? CriterioMigracao { get; set; }

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }
}
