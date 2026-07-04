using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Clientes.Domain;

namespace WebApolice.Modulos.Clientes.Application.Ports;

public interface IClientesRepository
{
    Task<Cliente?> ObterPorIdAsync(long id, CancellationToken cancellationToken);
    
    Task<Cliente?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken);
    
    Task<bool> ExisteCpfAsync(string cpf, CancellationToken cancellationToken);
    
    Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken);
    
    Task AtualizarAsync(Cliente cliente, CancellationToken cancellationToken);
    
    Task<(IReadOnlyList<Cliente> Itens, int TotalItens, int TotalPaginas)> ListarPaginadoAsync(
        int pagina, 
        int tamanhoPagina, 
        string? nome, 
        string? cpf, 
        StatusCliente? status, 
        string? ordenarPor, 
        string? direcao, 
        CancellationToken cancellationToken);
        
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
