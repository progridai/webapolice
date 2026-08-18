using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Atendimento.src.WebApolice.Modulos.Atendimento.Infrastructure.Persistence.Models;

public partial class ProtocoloItem
{
    public long Id { get; set; }

    public long ProtocoloLoteId { get; set; }

    public long? PessoaId { get; set; }

    public long? ClienteId { get; set; }

    public long? ClienteVinculoId { get; set; }

    public long? EstipulanteId { get; set; }

    public decimal? Premio { get; set; }

    public DateOnly? DataVigencia { get; set; }

    public string? Equipe { get; set; }

    public string? Matricula { get; set; }

    public string TipoItem { get; set; } = null!;

    public string? NomeConjuge { get; set; }

    public string OrigemLegado { get; set; } = null!;

    public int LegadoId { get; set; }

    public int? LegadoClienteId { get; set; }

    public int? LegadoEstipulanteId { get; set; }

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ProtocoloLote ProtocoloLote { get; set; } = null!;
}
