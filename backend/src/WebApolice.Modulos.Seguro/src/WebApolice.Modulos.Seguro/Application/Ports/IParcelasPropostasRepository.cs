using WebApolice.Modulos.Seguro.Domain.Modelos;

namespace WebApolice.Modulos.Seguro.Application.Ports;

public interface IParcelasPropostasRepository
{
    Task<WebApolice.Modulos.Cadastro.Contracts.ReferenciasPropostas> ObterReferenciasAsync(IReadOnlyList<Propostum> propostas, CancellationToken ct);
    Task<PropostaFinanceiro?> ObterPlanoAsync(long propostaId, CancellationToken ct);
    Task<IReadOnlyList<PropostaFinanceiro>> ObterPlanosAsync(long apoliceId, CancellationToken ct);
    Task<IReadOnlyList<Propostum>> ObterPropostasAsync(long apoliceId, CancellationToken ct);
    Task<IReadOnlyList<PropostaMovimento>> ObterMovimentosAsync(long apoliceId, CancellationToken ct);
    Task<IReadOnlyList<ParcelaCompetencia>> ObterCompetenciasAsync(long apoliceId, CancellationToken ct);
    Task<PropostaMovimento?> ObterMovimentoAsync(Guid publicId, CancellationToken ct);
    void Adicionar(PropostaFinanceiro plano);
    void Adicionar(PropostaMovimento movimento);
    void Adicionar(ParcelaCompetencia competencia);
}
