using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarSeguradora;
using WebApolice.Modulos.Cadastro.Application.UseCases.ListarSeguradoras;

namespace WebApolice.Modulos.Cadastro.Application.Ports;

public interface ISeguradorasQueries
{
    Task<(IEnumerable<SeguradoraListagemItemResult> itens, int totalItens, int totalPaginas)> ListarPaginadoAsync(
        int pagina, 
        int tamanhoPagina, 
        string? busca, 
        bool? ativo, 
        CancellationToken cancellationToken);

    Task<SeguradoraDetalheResult?> ObterPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
}
