using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.Domain.Modelos;

public partial class PropostaHistorico
{
    public long Id { get; set; }

    public long PropostaAnteriorId { get; set; }

    public long PropostaNovaId { get; set; }

    public string? Motivo { get; set; }

    public string? Observacao { get; set; }

    public DateTime DataAlteracao { get; set; }

    public string? LegadoOrigem { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Propostum PropostaAnterior { get; set; } = null!;

    public virtual Propostum PropostaNova { get; set; } = null!;
}
