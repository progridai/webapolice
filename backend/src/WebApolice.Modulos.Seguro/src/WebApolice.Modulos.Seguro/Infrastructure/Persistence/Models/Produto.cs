using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public partial class Produto
{
    public long Id { get; set; }

    public long? TabelaPrecoId { get; set; }

    public long? PlanoId { get; set; }

    public string? Nome { get; set; }

    public string? CodigoReferencia { get; set; }

    public string? Ramo { get; set; }

    public bool? GeraConjuge { get; set; }

    public bool? PagaComissao { get; set; }

    public int? LegadoId { get; set; }

    public int? LegadoProdutoAnt { get; set; }

    public bool Ativo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Plano? Plano { get; set; }

    public virtual ICollection<PropostaItem> PropostaItems { get; set; } = new List<PropostaItem>();

    public virtual TabelaPreco? TabelaPreco { get; set; }
}
