using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Comissao.src.WebApolice.Modulos.Comissao.Infrastructure.Persistence.Models;

public partial class CorretoraAgenciador
{
    public long Id { get; set; }

    public long? CorretoraId { get; set; }

    public long? AgenciadorId { get; set; }

    public decimal? PercentualAgenciamento { get; set; }

    public decimal? PercentualRepasse { get; set; }

    public DateOnly? InicioVigencia { get; set; }

    public DateOnly? FimVigencia { get; set; }

    public bool Ativo { get; set; }

    public int? LegadoId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
