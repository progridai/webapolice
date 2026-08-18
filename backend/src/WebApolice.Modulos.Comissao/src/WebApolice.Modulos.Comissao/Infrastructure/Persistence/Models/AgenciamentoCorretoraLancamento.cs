using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Comissao.src.WebApolice.Modulos.Comissao.Infrastructure.Persistence.Models;

public partial class AgenciamentoCorretoraLancamento
{
    public long Id { get; set; }

    public long? PropostaId { get; set; }

    public long? CorretoraId { get; set; }

    public long? MovimentoTipoId { get; set; }

    public decimal? Percentual { get; set; }

    public decimal? ValorPremio { get; set; }

    public decimal? ValorAgenciamento { get; set; }

    public int? ParcelaInicial { get; set; }

    public int? ParcelaFinal { get; set; }

    public int? StatusLegado { get; set; }

    public decimal? ValorPago { get; set; }

    public DateOnly? DataPagamento { get; set; }

    public bool? GerouFatura { get; set; }

    public DateOnly? DataCadastro { get; set; }

    public DateOnly? DataVencimento { get; set; }

    public int LegadoId { get; set; }

    public int? LegadoPropostaId { get; set; }

    public int? LegadoCorretoraId { get; set; }

    public int? LegadoMovimentoId { get; set; }

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
