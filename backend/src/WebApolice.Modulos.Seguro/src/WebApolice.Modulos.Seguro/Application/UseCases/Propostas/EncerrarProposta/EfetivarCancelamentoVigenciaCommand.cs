using MediatR;
using Microsoft.Extensions.Options;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.GerirAgendamento;
using WebApolice.SharedKernel.Application.Exceptions;
using WebApolice.SharedKernel.Text;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.EncerrarProposta;

public sealed record EfetivarCancelamentoVigenciaCommand(Guid PublicId, int? RevisaoEsperada) : IRequest<CancelamentoVigenciaResultado>;

public sealed class EfetivarCancelamentoVigenciaHandler(IPropostasRepository repository, IOperacaoPropostas operacao,
    IOptions<ConfiguracaoPropostas> configuracao, TimeProvider relogio) : IRequestHandler<EfetivarCancelamentoVigenciaCommand, CancelamentoVigenciaResultado>
{
    public async Task<CancelamentoVigenciaResultado> Handle(EfetivarCancelamentoVigenciaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return await operacao.ExecutarAsync("Efetivar Cancelamento de Vigência", async ct =>
            {
                if (request.RevisaoEsperada is null or < 0) throw new ValidacaoException("Informe a revisão da proposta.");
                var p = await repository.ObterParaPreenchimentoAsync(request.PublicId, ct) ?? throw new ValidacaoException("Proposta não encontrada.");
                if (p.LegadoId != null || p.DeletedAt != null || !p.VisivelOperacional || p.CancelamentoSolicitadoEm == null || p.DataCancelamentoVigencia == null)
                    throw new ValidacaoException("Proposta sem solicitação nativa de cancelamento de vigência.");
                if (p.CanceladaEm != null && !p.Vigente && p.DataFimVigencia == p.DataCancelamentoVigencia)
                    throw new SemAlteracao(new(true, false, p.RevisaoPreenchimento));
                if (p.RevisaoPreenchimento != request.RevisaoEsperada) throw new ValidacaoException("A proposta foi alterada. Recarregue antes de efetivar o cancelamento.");
                if (p.DataCancelamentoVigencia > VigenciaPropostas.Hoje(relogio)) throw new ValidacaoException("A data de cancelamento ainda não chegou.");
                var antes = VigenciaPropostas.Retrato(p); var agora = relogio.GetUtcNow().UtcDateTime;
                Domain.Modelos.PropostaStatus status;
                try
                {
                    await EncerramentoPropostas.ValidarVigenteAsync(p, repository, configuracao.Value, ct);
                    if (p.DataCancelamentoVigencia < p.DataInicioVigencia) throw new ValidacaoException("O cancelamento não pode anteceder o início registrado da proposta.");
                    status = await EncerramentoPropostas.ObterStatusAsync(repository, configuracao.Value, false, ct);
                }
                catch (ValidacaoException ex)
                {
                    var motivo = PadronizacaoTexto.TitleCase(ex.Message)!;
                    if (p.MotivoPendenciaVigencia == motivo) throw new SemAlteracao(new(false, false, p.RevisaoPreenchimento, motivo));
                    p.MotivoPendenciaVigencia = motivo; p.PendenciaVigenciaEm = agora; p.UpdatedAt = agora;
                    p.RevisaoPreenchimento = checked(p.RevisaoPreenchimento + 1);
                    await repository.SalvarAlteracoesAsync(ct);
                    return (new CancelamentoVigenciaResultado(false, false, p.RevisaoPreenchimento, motivo), p.PublicId.ToString(), antes, VigenciaPropostas.Retrato(p));
                }
                EncerramentoPropostas.Concluir(p, status, agora);
                GestaoAgendamento.Registrar(p, "Efetivação do Cancelamento de Vigência", $"Primeiro Dia sem Vigência: {p.DataCancelamentoVigencia:dd/MM/yyyy}.", agora);
                await repository.SalvarAlteracoesAsync(ct);
                return (new CancelamentoVigenciaResultado(true, false, p.RevisaoPreenchimento), p.PublicId.ToString(), antes, VigenciaPropostas.Retrato(p));
            }, cancellationToken);
        }
        catch (SemAlteracao ex) { return ex.Resultado; }
    }
    private sealed class SemAlteracao(CancelamentoVigenciaResultado resultado) : Exception
    { internal CancelamentoVigenciaResultado Resultado { get; } = resultado; }
}
