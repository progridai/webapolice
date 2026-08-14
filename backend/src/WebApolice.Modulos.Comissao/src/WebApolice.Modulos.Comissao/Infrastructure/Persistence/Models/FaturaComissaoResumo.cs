using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Comissao.src.WebApolice.Modulos.Comissao.Infrastructure.Persistence.Models;

public partial class FaturaComissaoResumo
{
    public long Id { get; set; }

    public long? EstipulanteId { get; set; }

    public string? Mes { get; set; }

    public string? Ano { get; set; }

    public int? CompetenciaInt { get; set; }

    public decimal? PremioPagamento { get; set; }

    public decimal? ValorPago { get; set; }

    public DateOnly? DataPagamento { get; set; }

    public int LegadoId { get; set; }

    public int? LegadoEstipulanteId { get; set; }

    public DateTime CreatedAt { get; set; }
}
