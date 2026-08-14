using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Comissao.src.WebApolice.Modulos.Comissao.Infrastructure.Persistence.Models;

public partial class FaturaVidaRecebimento
{
    public long Id { get; set; }

    public long? FaturaVidaAgenciamentoId { get; set; }

    public long? EstipulanteId { get; set; }

    public DateOnly? DataPagamento { get; set; }

    public decimal? Valor { get; set; }

    public string? Observacao { get; set; }

    public int LegadoId { get; set; }

    public int? LegadoFaturaVidaId { get; set; }

    public int? LegadoEstipulanteId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual FaturaVidaAgenciamento? FaturaVidaAgenciamento { get; set; }
}
