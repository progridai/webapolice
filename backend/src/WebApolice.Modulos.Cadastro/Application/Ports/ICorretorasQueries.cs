using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarCorretora;
using WebApolice.Modulos.Cadastro.Application.UseCases.ListarCorretoras;

namespace WebApolice.Modulos.Cadastro.Application.Ports;

public interface ICorretorasQueries
{
    Task<(IEnumerable<CorretoraListagemItemResult> itens, int totalItens, int totalPaginas)> ListarPaginadoAsync(
        int pagina, 
        int tamanhoPagina, 
        string? busca, 
        bool? ativo, 
        CancellationToken cancellationToken);
        
    Task<CorretoraDetalheResult?> ObterPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
}
