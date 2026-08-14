using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence.Models;

public partial class MovimentoCobrancaLog
{
    public long Id { get; set; }

    public long? PropostaMovimentoId { get; set; }

    public long? TituloId { get; set; }

    public int? UsuarioLegadoId { get; set; }

    public DateTime? DataMovimento { get; set; }

    public DateOnly? DataPagamento { get; set; }

    public decimal? ValorPagamento { get; set; }

    public DateTime? DataAlteracao { get; set; }

    public int LegadoMovimentoPropostaId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Titulo? Titulo { get; set; }
}
