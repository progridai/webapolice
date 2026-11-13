using System.Data.Common;

namespace WebApolice.Modulos.Cadastro.Contracts;

/// <summary>Criação contextual de vínculos na mesma transação física da participação.</summary>
public interface ICadastroVinculosVida
{
    Task<VinculoClienteSeguro> CriarAsync(long clienteId, long estipulanteId, string matricula, CancellationToken ct);
    Task<VinculoClienteSeguro> CompletarAsync(long clienteId, Guid vinculoPublicId, long estipulanteId, CancellationToken ct);
    Task UsarTransacaoAsync(DbTransaction? transacao, CancellationToken ct);
    void DescartarAlteracoes();
}
