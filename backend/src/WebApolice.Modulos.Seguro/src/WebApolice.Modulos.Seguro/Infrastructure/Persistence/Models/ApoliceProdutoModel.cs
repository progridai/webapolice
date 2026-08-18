using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public class ApoliceProdutoModel
{
    public long Id { get; set; }
    public long ApoliceId { get; set; }
    
    // FK Global
    public long ProdutoId { get; set; }

    public bool Ativo { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navegações
    public ApoliceModel? Apolice { get; set; }
    public Produto? Produto { get; set; }
    
    public ICollection<ApolicePlanoModel> Planos { get; set; } = new List<ApolicePlanoModel>();
}
