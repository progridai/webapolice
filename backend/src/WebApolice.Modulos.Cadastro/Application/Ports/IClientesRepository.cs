using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Domain;

namespace WebApolice.Modulos.Cadastro.Application.Ports;

public interface IClientesRepository
{
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken);
    Task AtualizarAsync(Cliente cliente, CancellationToken cancellationToken);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
