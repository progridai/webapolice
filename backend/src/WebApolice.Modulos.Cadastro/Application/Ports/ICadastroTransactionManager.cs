using System.Threading;
using System.Threading.Tasks;

namespace WebApolice.Modulos.Cadastro.Application.Ports;

public interface ICadastroTransactionManager
{
    Task<System.Data.Common.DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
