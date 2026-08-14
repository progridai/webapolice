using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Comissao.src.WebApolice.Modulos.Comissao.Infrastructure.Persistence.Models;

public partial class LancamentoFaturaEstipulante
{
    public long Id { get; set; }

    public long? EstipulanteId { get; set; }

    public long? CorretoraId { get; set; }

    public string? CompetenciaOriginal { get; set; }

    public int? CompetenciaMes { get; set; }

    public int? CompetenciaAno { get; set; }

    public int? CompetenciaInt { get; set; }

    public decimal? PremioTotal { get; set; }

    public decimal? ValorFaturado { get; set; }

    public decimal? PercentualCorretagem { get; set; }

    public decimal? ComissaoRecebida { get; set; }

    public DateOnly? DataVencimentoFatura { get; set; }

    public DateOnly? DataRecebimento { get; set; }

    public bool? LancamentoManual { get; set; }

    public int LegadoId { get; set; }

    public int? LegadoEstipulanteId { get; set; }

    public int? LegadoCorretoraId { get; set; }

    public DateTime CreatedAt { get; set; }
}
