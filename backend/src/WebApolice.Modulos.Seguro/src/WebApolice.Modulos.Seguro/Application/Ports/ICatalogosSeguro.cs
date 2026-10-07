namespace WebApolice.Modulos.Seguro.Application.Ports;

public class CoberturaDados
{
    public string Nome { get; set; } = string.Empty;
    public string? NomeReduzido { get; set; }
    public string? Basica { get; set; }
    public bool? Reajuste { get; set; }
}
public class PlanoDados
{
    public string Nome { get; set; } = string.Empty;
    public string? Ramo { get; set; }
    public bool? Paga { get; set; }
    public bool? Reajuste { get; set; }
}
public sealed record CoberturaDto(Guid PublicId, string Nome, string? NomeReduzido, string? Basica, bool? Reajuste, bool Ativo, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record PlanoDto(Guid PublicId, string Nome, string? Ramo, bool? Paga, bool? Reajuste, bool Ativo, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record CatalogoPagina<T>(IReadOnlyList<T> Items, int TotalCount);
public sealed record PlanoCoberturaDto(Guid CoberturaPublicId, string Nome, bool Ativo, bool CoberturaAtiva, decimal? PremioTitular = null, decimal? PremioConjuge = null);
public class PremiosCoberturaDados
{
    public decimal? PremioTitular { get; set; }
    public decimal? PremioConjuge { get; set; }
}
public sealed class PlanoCoberturaDados : PremiosCoberturaDados
{
    public bool Ativo { get; set; } = true;
}
public interface ICatalogosSeguro
{
    Task<CatalogoPagina<CoberturaDto>> ListarCoberturasAsync(int pagina, int tamanho, string? busca, bool? ativo, CancellationToken ct);
    Task<CatalogoPagina<PlanoDto>> ListarPlanosAsync(int pagina, int tamanho, string? busca, bool? ativo, CancellationToken ct);
    Task<CoberturaDto?> ObterCoberturaAsync(Guid id, CancellationToken ct);
    Task<PlanoDto?> ObterPlanoAsync(Guid id, CancellationToken ct);
    Task<CoberturaDto> SalvarCoberturaAsync(Guid? id, CoberturaDados dados, CancellationToken ct);
    Task<PlanoDto> SalvarPlanoAsync(Guid? id, PlanoDados dados, CancellationToken ct);
    Task StatusAsync(Guid id, bool ativo, bool cobertura, CancellationToken ct);
    Task<IReadOnlyList<PlanoCoberturaDto>> CoberturasPlanoAsync(Guid id, CancellationToken ct);
    Task VincularAsync(Guid planoId, Guid coberturaId, bool ativo, CancellationToken ct);
    Task SalvarVinculoAsync(Guid planoId, Guid coberturaId, PlanoCoberturaDados dados, CancellationToken ct);
}
