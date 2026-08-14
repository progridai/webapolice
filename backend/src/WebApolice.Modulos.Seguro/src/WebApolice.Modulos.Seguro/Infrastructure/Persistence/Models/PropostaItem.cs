using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public partial class PropostaItem
{
    public long Id { get; set; }

    public long PropostaId { get; set; }

    public long? TipoProdutoId { get; set; }

    public long? TabelaPrecoId { get; set; }

    public long? ProdutoId { get; set; }

    public long? PlanoId { get; set; }

    public string? PlanoCodigoLegado { get; set; }

    public string? PlanoNomeLegado { get; set; }

    public string? Ramo { get; set; }

    public decimal? Valor { get; set; }

    public bool? PagaComissao { get; set; }

    public int? CodigoLegado { get; set; }

    public int? CdMovVid { get; set; }

    public int? UltimaFaixaEtaria { get; set; }

    public int LegadoId { get; set; }

    public int? LegadoPropostaTipoAnt { get; set; }

    public bool Ativo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Plano? Plano { get; set; }

    public virtual Produto? Produto { get; set; }

    public virtual Propostum Proposta { get; set; } = null!;

    public virtual ICollection<PropostaCobertura> PropostaCoberturas { get; set; } = new List<PropostaCobertura>();

    public virtual TabelaPreco? TabelaPreco { get; set; }

    public virtual TipoProduto? TipoProduto { get; set; }
}
