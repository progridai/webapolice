using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Comissao.src.WebApolice.Modulos.Comissao.Infrastructure.Persistence.Models;

public partial class EstipulanteComissaoConfig
{
    public long Id { get; set; }

    public long EstipulanteId { get; set; }

    public decimal? PercentualComissao { get; set; }

    public decimal? PercentualAgenciamento { get; set; }

    public decimal? PercentualBonus { get; set; }

    public int? ComissaoApartirParcela { get; set; }

    public long? AgenciadorId { get; set; }

    public decimal? AgenciadorPercentualRepasse { get; set; }

    public bool Ativo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
