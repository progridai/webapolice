using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public partial class PropostaBeneficiario
{
    public long Id { get; set; }

    public long PropostaId { get; set; }

    public long? PessoaId { get; set; }

    public string? Nome { get; set; }

    public string? NomeNormalizado { get; set; }

    public string? CpfOriginal { get; set; }

    public string? CpfLimpo { get; set; }

    public bool CpfValido { get; set; }

    public string? ParentescoOriginal { get; set; }

    public string? ParentescoNormalizado { get; set; }

    public decimal? PercentualParticipacao { get; set; }

    public int? Ordem { get; set; }

    public string? Observacao { get; set; }

    public int LegadoId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Propostum Proposta { get; set; } = null!;
}
