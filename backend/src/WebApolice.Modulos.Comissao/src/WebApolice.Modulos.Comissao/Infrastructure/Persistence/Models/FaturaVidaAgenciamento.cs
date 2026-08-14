using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Comissao.src.WebApolice.Modulos.Comissao.Infrastructure.Persistence.Models;

public partial class FaturaVidaAgenciamento
{
    public long Id { get; set; }

    public string OrigemLegado { get; set; } = null!;

    public long? PropostaId { get; set; }

    public decimal? Premio { get; set; }

    public decimal? Iof { get; set; }

    public decimal? PremioLiquido { get; set; }

    public decimal? ValorAgenciamento { get; set; }

    public decimal? ValorRecebido { get; set; }

    public decimal? ValorDiferenca { get; set; }

    public string? CodigoCooperadoOriginal { get; set; }

    public string? CodigoCorretoraOriginal { get; set; }

    public string? TipoAgenciamento { get; set; }

    public string? NumeroNf { get; set; }

    public DateTime? DataInclusao { get; set; }

    public DateTime? DataRegistro { get; set; }

    public int LegadoId { get; set; }

    public int? LegadoPropostaId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<FaturaVidaRecebimento> FaturaVidaRecebimentos { get; set; } = new List<FaturaVidaRecebimento>();
}
