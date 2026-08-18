using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public class ApolicePlanoModel
{
    public long Id { get; set; }
    public long ApoliceProdutoId { get; set; }
    
    // FK Global
    public long PlanoId { get; set; }

    // Override opcional de preço da tabela global
    public long? TabelaPrecoId { get; set; }

    public bool Ativo { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navegações
    public ApoliceProdutoModel? ApoliceProduto { get; set; }
    public Plano? Plano { get; set; }
    public TabelaPreco? TabelaPreco { get; set; }
    
    public ICollection<ApoliceCoberturaModel> Coberturas { get; set; } = new List<ApoliceCoberturaModel>();
}
