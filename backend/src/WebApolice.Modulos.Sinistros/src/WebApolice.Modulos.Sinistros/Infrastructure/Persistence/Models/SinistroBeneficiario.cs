using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Sinistros.src.WebApolice.Modulos.Sinistros.Infrastructure.Persistence.Models;

public partial class SinistroBeneficiario
{
    public long Id { get; set; }

    public long SinistroId { get; set; }

    public long? PropostaId { get; set; }

    public long? PropostaBeneficiarioId { get; set; }

    public long? PessoaId { get; set; }

    public string? Nome { get; set; }

    public string? CpfOriginal { get; set; }

    public string? CpfLimpo { get; set; }

    public bool CpfValido { get; set; }

    public string? ParentescoOriginal { get; set; }

    public decimal? PercentualParticipacao { get; set; }

    public decimal? ValorPago { get; set; }

    public string? Observacao { get; set; }

    public int? LegadoId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Sinistro Sinistro { get; set; } = null!;
}
