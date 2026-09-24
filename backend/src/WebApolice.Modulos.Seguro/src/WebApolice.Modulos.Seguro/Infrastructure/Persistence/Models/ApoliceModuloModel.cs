using System;
using System.Collections.Generic;
namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public class ApoliceModuloModel
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    
    public long ApoliceId { get; set; }
    public long ModuloId { get; set; }
    
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    
    public bool Ativo { get; set; }
    public string? Observacao { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // Navigation properties
    public ApoliceModel? Apolice { get; set; }
    public ICollection<ApoliceVidaModel> Vidas { get; set; } = new List<ApoliceVidaModel>();
}
