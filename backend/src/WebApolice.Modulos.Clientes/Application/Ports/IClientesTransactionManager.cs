using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApolice.Modulos.Clientes.Application.Ports;

public interface IClientesTransactionManager
{
    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
}
