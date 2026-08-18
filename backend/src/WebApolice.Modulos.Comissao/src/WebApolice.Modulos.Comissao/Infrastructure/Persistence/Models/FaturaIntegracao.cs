using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Comissao.src.WebApolice.Modulos.Comissao.Infrastructure.Persistence.Models;

public partial class FaturaIntegracao
{
    public long Id { get; set; }

    public long? CorretoraId { get; set; }

    public long? SeguradoraId { get; set; }

    public long? EstipulanteId { get; set; }

    public string? CorretoraCodigoOriginal { get; set; }

    public string? SeguradoraCodigoOriginal { get; set; }

    public DateTime? DataLancamento { get; set; }

    public DateOnly? DataVencimento { get; set; }

    public DateOnly? DataRecebimento { get; set; }

    public decimal? ValorReceber { get; set; }

    public decimal? ValorRecebido { get; set; }

    public decimal? ValorFatura { get; set; }

    public int? SituacaoLegado { get; set; }

    public string? Tipo { get; set; }

    public int? Mes { get; set; }

    public int? Ano { get; set; }

    public int? CompetenciaInt { get; set; }

    public bool? GerouArquivo { get; set; }

    public int? Alterado { get; set; }

    public decimal? PercentualAgenciamento { get; set; }

    public decimal? PercentualCorretagem { get; set; }

    public int LegadoId { get; set; }

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
