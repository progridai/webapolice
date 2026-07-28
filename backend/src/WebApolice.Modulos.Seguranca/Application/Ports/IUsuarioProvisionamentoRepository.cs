using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguranca.Domain;

namespace WebApolice.Modulos.Seguranca.Application.Ports;

public interface IUsuarioProvisionamentoRepository
{
    Task<Usuario?> ObterPorKeycloakSubParaAtualizacaoAsync(string keycloakSub, CancellationToken cancellationToken);
    Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
    void LimparRastreamento();
}
