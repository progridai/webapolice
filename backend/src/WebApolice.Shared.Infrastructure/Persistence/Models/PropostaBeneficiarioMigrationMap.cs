using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class PropostaBeneficiarioMigrationMap
{
    public long Id { get; set; }

    public int LegadoBeneficiarioId { get; set; }

    public long PropostaBeneficiarioId { get; set; }

    public int? LegadoPropostaId { get; set; }

    public long? PropostaId { get; set; }

    public long? PessoaId { get; set; }

    public string? NomeOriginal { get; set; }

    public string? CpfOriginal { get; set; }

    public string? CpfLimpo { get; set; }

    public bool CpfValido { get; set; }

    public string? ParentescoOriginal { get; set; }

    public string? ParentescoNormalizado { get; set; }

    public string? CriterioMigracao { get; set; }

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }
}
