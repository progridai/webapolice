using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarSubestipulante;
using WebApolice.Modulos.Cadastro.Application.UseCases.ListarSubestipulantes;

namespace WebApolice.Modulos.Cadastro.Application.Ports;

public interface ISubestipulantesQueries
{
    Task<(IEnumerable<SubestipulanteListagemItemResult> itens, int totalItens, int totalPaginas)> ListarPaginadoAsync(
        int pagina,
        int tamanhoPagina,
        string? busca,
        bool? ativo,
        CancellationToken cancellationToken);

    Task<SubestipulanteDetalheResult?> ObterPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
}
