namespace WebApolice.Modulos.Seguro.Application.Ports;

public sealed class PlanoModuloDados : PlanoDados
{
    public bool Ativo { get; set; } = true;
}

public sealed class CoberturaModuloDados
{
    public Guid CoberturaPublicId { get; set; }
    public decimal? PremioTitular { get; set; }
    public decimal? PremioConjuge { get; set; }
    public bool Ativo { get; set; } = true;
}

public sealed record CoberturaModuloDto(Guid PublicId, Guid CoberturaPublicId, bool CoberturaAtiva, string Nome, string? NomeReduzido,
    string? Basica, bool? Reajuste, decimal PremioTitular, decimal PremioConjuge, bool Ativo);
public sealed record PlanoModuloDto(Guid PublicId, string Nome, string? Ramo, bool? Paga,
    bool? Reajuste, bool Ativo, IReadOnlyList<CoberturaModuloDto> Coberturas);
public sealed record ConfiguracaoPlanoModuloDto(Guid ApoliceModuloPublicId, bool PodeAlterar, PlanoModuloDto? Plano);

public sealed record CoberturaModuloOpcao(Guid PublicId, string Nome);

public interface IPlanoModuloApolice
{
    Task<CatalogoPagina<CoberturaModuloOpcao>> OpcoesCoberturasAsync(Guid apolice, Guid modulo, int pagina, int tamanho, CancellationToken ct);
    Task<ConfiguracaoPlanoModuloDto> ObterAsync(Guid apolice, Guid modulo, CancellationToken ct);
    Task<PlanoModuloDto> SalvarPlanoAsync(Guid apolice, Guid modulo, PlanoModuloDados dados, CancellationToken ct);
    Task<CoberturaModuloDto> SalvarCoberturaAsync(Guid apolice, Guid modulo, Guid? cobertura,
        CoberturaModuloDados dados, CancellationToken ct);
}
