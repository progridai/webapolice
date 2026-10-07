namespace WebApolice.Modulos.Financeiro.Contracts;

public class ConvenioCobrancaDados
{
    public string? BancoCodigo { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Agencia { get; set; }
    public string? ContaCorrente { get; set; }
    public string? NomeEmpresa { get; set; }
    public string? CodigoEmpresa { get; set; }
    public int? NumeroArquivo { get; set; }
    public string? NomeInicialArquivo { get; set; }
    public string? ExtensaoArquivo { get; set; }
    public short? LayoutArquivo { get; set; }
    public string? LocalRemessaArquivo { get; set; }
    public string? LocalRetornoArquivo { get; set; }
    public bool? ComunicaVindi { get; set; }
    public string? Observacao { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? EstEndereco { get; set; }
    public string? EstNumero { get; set; }
    public string? EstBairro { get; set; }
    public string? EstComplemento { get; set; }
    public string? EstCep { get; set; }
    public string? EstCidade { get; set; }
    public string? EstUf { get; set; }
    public string? EstNome { get; set; }
}
public sealed class ConvenioCobrancaDto : ConvenioCobrancaDados
{
    public Guid PublicId { get; set; }
    public bool Ativo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public sealed record ConvenioCobrancaReferencia(long Id, Guid PublicId, string Nome, bool Ativo);
public sealed record ConvenioCobrancaOpcao(Guid PublicId, string Nome, bool Ativo);
public sealed class BancoOpcao
{
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
}
public sealed record ConveniosPagina(IReadOnlyList<ConvenioCobrancaDto> Items, int TotalCount);
public interface IConveniosCobrancaConsulta
{
    Task<ConvenioCobrancaReferencia?> ObterAsync(Guid publicId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ConvenioCobrancaReferencia>> ObterPorIdsAsync(IReadOnlyList<long> ids, CancellationToken cancellationToken);
    Task<IReadOnlyList<ConvenioCobrancaOpcao>> OpcoesAsync(CancellationToken cancellationToken);
}
