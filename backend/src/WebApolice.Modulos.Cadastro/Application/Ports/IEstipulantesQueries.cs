using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApolice.Modulos.Cadastro.Application.Ports;

public interface IEstipulantesQueries
{
    Task<(System.Collections.Generic.IEnumerable<UseCases.ConsultarEstipulante.EstipulanteDetalheResult> itens, int totalItens, int totalPaginas)> ListarPaginadoAsync(
        int pagina,
        int tamanhoPagina,
        string? nome,
        string? cnpj,
        CancellationToken cancellationToken);

    Task<UseCases.ConsultarEstipulante.EstipulanteDetalheResult?> ObterPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
    
    Task<object?> ObterConfiguracaoPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
}
