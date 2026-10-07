namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
public sealed class PlanoCoberturaModel
{
    public long Id { get; set; }
    public long PlanoId { get; set; }
    public long CoberturaId { get; set; }
    public decimal? PremioTitular { get; set; }
    public decimal? PremioConjuge { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Plano Plano { get; set; } = null!;
    public Cobertura Cobertura { get; set; } = null!;
}
