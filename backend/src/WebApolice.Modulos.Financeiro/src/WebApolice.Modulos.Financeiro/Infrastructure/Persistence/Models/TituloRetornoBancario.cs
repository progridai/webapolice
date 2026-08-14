using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence.Models;

public partial class TituloRetornoBancario
{
    public long Id { get; set; }

    public long? TituloId { get; set; }

    public long? PropostaMovimentoId { get; set; }

    public long? RetornoCodigoId { get; set; }

    public string? CodigoOriginal { get; set; }

    public string? DescricaoOriginal { get; set; }

    public DateOnly? DataRetorno { get; set; }

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual RetornoBancarioCodigo? RetornoCodigo { get; set; }

    public virtual Titulo? Titulo { get; set; }
}
