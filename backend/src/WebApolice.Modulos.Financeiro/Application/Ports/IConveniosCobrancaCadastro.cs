using WebApolice.Modulos.Financeiro.Contracts;
namespace WebApolice.Modulos.Financeiro.Application.Ports;
public interface IConveniosCobrancaCadastro
{
    Task<ConveniosPagina> ListarAsync(int pagina, int tamanhoPagina, string? busca, bool? ativo, CancellationToken ct);
    Task<ConvenioCobrancaDto?> ConsultarAsync(Guid publicId, CancellationToken ct);
    Task<IReadOnlyList<BancoOpcao>> BancosAsync(CancellationToken ct);
    Task<ConvenioCobrancaDto> SalvarAsync(Guid? publicId, ConvenioCobrancaDados dados, CancellationToken ct);
    Task AlterarStatusAsync(Guid publicId, bool ativo, CancellationToken ct);
}
