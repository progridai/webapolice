using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarCliente;
using WebApolice.Modulos.Cadastro.Application.UseCases.ListarClientes;

namespace WebApolice.Modulos.Cadastro.Application.Ports;

public interface IClientesQueries
{
    Task<(ClienteListagemItemResult[] Itens, int TotalItens, int TotalPaginas)> ListarPaginadoAsync(
        int pagina, 
        int tamanhoPagina, 
        string? nome, 
        string? documento, 
        int? statusId, 
        string? ordenarPor, 
        string? direcao, 
        CancellationToken cancellationToken);

    Task<ConsultarClienteResult?> ObterDetalheAsync(Guid id, CancellationToken cancellationToken);
}
