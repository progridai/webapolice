using MediatR;
using Microsoft.Extensions.Options;
using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.SharedKernel.Application.Exceptions;
using WebApolice.SharedKernel.Text;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.EfetivarVigencia;

public sealed record EfetivarVigenciaPropostaCommand(Guid PublicId, int? RevisaoEsperada) : IRequest<EfetivarVigenciaResultado>;
public sealed record EfetivarVigenciaResultado(bool Ativada, bool JaEfetivada, int RevisaoPreenchimento, string? PendenciaVigencia);

public sealed class EfetivarVigenciaPropostaHandler(IPropostasRepository repository, IOperacaoPropostas operacao,
    IClientesPropostas clientes, ICadastroSeguroQueries cadastro, IOptions<ConfiguracaoPropostas> configuracao, TimeProvider relogio)
    : IRequestHandler<EfetivarVigenciaPropostaCommand, EfetivarVigenciaResultado>
{
    public async Task<EfetivarVigenciaResultado> Handle(EfetivarVigenciaPropostaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return await operacao.ExecutarAsync("Efetivar Vigência", async ct =>
            {
                if (request.PublicId == Guid.Empty || request.RevisaoEsperada is null or < 0)
                    throw new ValidacaoException("Informe a revisão da proposta para efetivar a vigência.");
                var p = await repository.ObterParaPreenchimentoAsync(request.PublicId, ct) ?? throw new ValidacaoException("Proposta não encontrada.");
                var codigo = configuracao.Value.StatusConfirmadaCodigo;
                if (string.IsNullOrWhiteSpace(codigo) || p.DescartadaEm != null || p.CancelamentoSolicitadoEm != null || p.CanceladaEm != null || p.LegadoId != null || p.ConfirmadaEm == null || p.DataInicioVigencia == null ||
                    p.DeletedAt != null || !p.VisivelOperacional || p.Status.Codigo != codigo || p.Status.Finalizador || p.Status.PermiteMovimentacao || p.SubstituidaEm != null)
                    throw new ValidacaoException("Somente uma proposta nativa confirmada e ainda não substituída pode ter sua vigência efetivada.");
                if (p.Vigente) throw new SemAlteracao(new(true, true, p.RevisaoPreenchimento, null));
                if (p.RevisaoPreenchimento != request.RevisaoEsperada) throw new ValidacaoException("A proposta foi alterada em outra tela. Recarregue antes de efetivar a vigência.");
                var hoje = VigenciaPropostas.Hoje(relogio);
                if (p.DataInicioVigencia > hoje) throw new ValidacaoException("A data de início ainda não chegou. A proposta anterior continuará vigente até a ativação da nova versão.");
                var antes = VigenciaPropostas.Retrato(p); var agora = relogio.GetUtcNow().UtcDateTime;
                Domain.Modelos.Propostum? origem;
                try
                {
                    await VigenciaPropostas.ValidarContextoAsync(p, repository, clientes, cadastro, ct);
                    VigenciaPropostas.ValidarDatas(p, p.DataInicioVigencia.Value, hoje, efetivar: true);
                    origem = await VigenciaPropostas.ValidarOrigemAsync(p, p.DataInicioVigencia.Value, repository, configuracao.Value, ct);
                }
                catch (ValidacaoException ex)
                {
                    var motivo = PadronizacaoTexto.TitleCase(ex.Message)!;
                    if (p.MotivoPendenciaVigencia == motivo) throw new SemAlteracao(new(false, false, p.RevisaoPreenchimento, motivo));
                    p.MotivoPendenciaVigencia = motivo; p.PendenciaVigenciaEm = agora; p.UpdatedAt = agora;
                    p.RevisaoPreenchimento = checked(p.RevisaoPreenchimento + 1);
                    await repository.SalvarAlteracoesAsync(ct);
                    return (new EfetivarVigenciaResultado(false, false, p.RevisaoPreenchimento, motivo), p.PublicId.ToString(), antes, VigenciaPropostas.Retrato(p));
                }
                antes = VigenciaPropostas.Retrato(p, origem);
                p.RevisaoPreenchimento = checked(p.RevisaoPreenchimento + 1); p.UpdatedAt = agora;
                await VigenciaPropostas.EfetivarAsync(p, origem, agora, repository, ct);
                await repository.SalvarAlteracoesAsync(ct);
                return (new EfetivarVigenciaResultado(true, false, p.RevisaoPreenchimento, null), p.PublicId.ToString(), antes, VigenciaPropostas.Retrato(p, origem));
            }, cancellationToken);
        }
        catch (SemAlteracao ex) { return ex.Resultado; }
    }

    private sealed class SemAlteracao(EfetivarVigenciaResultado resultado) : Exception
    { internal EfetivarVigenciaResultado Resultado { get; } = resultado; }
}
