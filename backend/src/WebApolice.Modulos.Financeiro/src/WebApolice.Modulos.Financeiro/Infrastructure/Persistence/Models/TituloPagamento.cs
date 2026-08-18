using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence.Models;

public partial class TituloPagamento
{
    public long Id { get; set; }

    public long TituloId { get; set; }

    public long? PropostaMovimentoId { get; set; }

    public DateOnly? DataPagamento { get; set; }

    public decimal ValorPago { get; set; }

    public string? FormaPagamento { get; set; }

    public string Origem { get; set; } = null!;

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Titulo Titulo { get; set; } = null!;
}
