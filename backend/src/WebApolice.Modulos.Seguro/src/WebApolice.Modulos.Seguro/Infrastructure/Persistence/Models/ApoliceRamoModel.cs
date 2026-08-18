using System;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public class ApoliceRamoModel
{
    public long Id { get; set; }
    public long ApoliceId { get; set; }
    
    public string TipoRamo { get; set; } = string.Empty;
    public string? NumeroApolice { get; set; }
    public decimal? IofPercentual { get; set; }
    
    public bool Ativo { get; set; }
    public int? LegadoId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Propriedades de Navegação (EF)
    public ApoliceModel? Apolice { get; set; }
}
