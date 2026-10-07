namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public sealed class ApoliceModuloPlanoModel
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long ApoliceModuloId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Ramo { get; set; }
    public bool? Paga { get; set; }
    public bool? Reajuste { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ApoliceModuloModel ApoliceModulo { get; set; } = null!;
    public ICollection<ApoliceModuloCoberturaModel> Coberturas { get; set; } = new List<ApoliceModuloCoberturaModel>();
}

public sealed class ApoliceModuloCoberturaModel
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long ApoliceModuloPlanoId { get; set; }
    public long CoberturaId { get; set; }
    public Cobertura Cobertura { get; set; } = null!;
    public decimal PremioTitular { get; set; }
    public decimal PremioConjuge { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ApoliceModuloPlanoModel Plano { get; set; } = null!;
}
