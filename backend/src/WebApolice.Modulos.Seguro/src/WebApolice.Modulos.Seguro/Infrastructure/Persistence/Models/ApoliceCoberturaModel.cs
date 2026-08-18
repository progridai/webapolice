using System;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public class ApoliceCoberturaModel
{
    public long Id { get; set; }
    public long ApolicePlanoId { get; set; }
    
    // FK Global
    public long CoberturaId { get; set; }

    // Opcional: Overrides de importâncias seguradas ou preços específicos por apólice
    public decimal? ImportanciaSeguradaOverride { get; set; }
    public decimal? PremioOverride { get; set; }

    public bool Ativo { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navegações
    public ApolicePlanoModel? ApolicePlano { get; set; }
    public Cobertura? Cobertura { get; set; }
}
