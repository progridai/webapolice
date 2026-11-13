using MediatR;
using Microsoft.Extensions.Options;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.GerirAgendamento;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.EncerrarProposta;

public sealed record CancelarVigenciaDados(int? RevisaoEsperada, DateOnly? DataCancelamentoVigencia, string? Observacao);
public sealed record SolicitarCancelamentoVigenciaCommand(Guid PublicId, CancelarVigenciaDados Dados) : IRequest<CancelamentoVigenciaResultado>;
public sealed record CancelamentoVigenciaResultado(bool Cancelada, bool Agendada, int RevisaoPreenchimento, string? PendenciaVigencia = null);

public sealed class SolicitarCancelamentoVigenciaHandler(IPropostasRepository repository, IOperacaoPropostas operacao,
    IOptions<ConfiguracaoPropostas> configuracao, TimeProvider relogio)
    : IRequestHandler<SolicitarCancelamentoVigenciaCommand, CancelamentoVigenciaResultado>
{
    public Task<CancelamentoVigenciaResultado> Handle(SolicitarCancelamentoVigenciaCommand request, CancellationToken cancellationToken) =>
        operacao.ExecutarAsync("Solicitar Cancelamento de Vigência", async ct =>
        {
            var p = await repository.ObterParaPreenchimentoAsync(request.PublicId, ct) ?? throw new ValidacaoException("Proposta não encontrada.");
            EncerramentoPropostas.ValidarPedido(p, request.Dados.RevisaoEsperada, request.Dados.Observacao);
            if (p.CancelamentoSolicitadoEm != null || p.DataCancelamentoVigencia != null) throw new ValidacaoException("Esta proposta já possui cancelamento solicitado. Consulte a data registrada.");
            await EncerramentoPropostas.ValidarVigenteAsync(p, repository, configuracao.Value, ct);
            var status = await EncerramentoPropostas.ObterStatusAsync(repository, configuracao.Value, false, ct);
            var data = request.Dados.DataCancelamentoVigencia;
            if (data == null || data < p.DataInicioVigencia)
                throw new ValidacaoException("Informe a data de cancelamento, igual ou posterior ao início da proposta.");
            var hoje = VigenciaPropostas.Hoje(relogio); var agendada = data > hoje;
            try { VigenciaPropostas.ValidarDatas(p, data.Value, hoje, efetivar: false); }
            catch (ValidacaoException) { throw new ValidacaoException("A data de cancelamento deve estar dentro dos períodos da Apólice e da Vida."); }
            if (agendada && !configuracao.Value.ProcessarAgendamentos)
                throw new ValidacaoException("O processamento de cancelamentos futuros está desabilitado.");
            var antes = VigenciaPropostas.Retrato(p); var agora = relogio.GetUtcNow().UtcDateTime;
            p.DataCancelamentoVigencia = data; p.CancelamentoSolicitadoEm = agora;
            if (!agendada) EncerramentoPropostas.Concluir(p, status, agora);
            GestaoAgendamento.Registrar(p, agendada ? "Cancelamento de Vigência Agendado" : "Cancelamento de Vigência",
                $"Primeiro Dia sem Vigência: {data:dd/MM/yyyy}. Justificativa: {request.Dados.Observacao!.Trim()}", agora);
            await repository.SalvarAlteracoesAsync(ct);
            return (new CancelamentoVigenciaResultado(!agendada, agendada, p.RevisaoPreenchimento), p.PublicId.ToString(), antes, VigenciaPropostas.Retrato(p));
        }, cancellationToken);
}
