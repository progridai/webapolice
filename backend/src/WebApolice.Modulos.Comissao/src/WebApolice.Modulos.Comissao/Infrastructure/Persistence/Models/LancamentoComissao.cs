using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Comissao.src.WebApolice.Modulos.Comissao.Infrastructure.Persistence.Models;

public partial class LancamentoComissao
{
    public long Id { get; set; }

    public long? PropostaMovimentoId { get; set; }

    public long? TituloId { get; set; }

    public long? PropostaId { get; set; }

    public long? PessoaId { get; set; }

    public long? ClienteId { get; set; }

    public long? EstipulanteId { get; set; }

    public int? CompetenciaAno { get; set; }

    public int? CompetenciaMes { get; set; }

    public int? CompetenciaInt { get; set; }

    public decimal? ValorBase { get; set; }

    public decimal? ValorBruto { get; set; }

    public decimal? ValorLiquido { get; set; }

    public char? Gerado { get; set; }

    public string Status { get; set; } = null!;

    public string Origem { get; set; } = null!;

    public int? LegadoMovimentoPropostaId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
