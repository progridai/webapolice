using MediatR;
using Microsoft.Extensions.Options;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.GerirAgendamento;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.EncerrarProposta;

public sealed record AlterarCancelamentoVigenciaCommand(Guid PublicId, CancelarVigenciaDados Dados) : IRequest<CancelamentoVigenciaResultado>;
public sealed record RetirarCancelamentoVigenciaDados(int? RevisaoEsperada, string? Observacao);
public sealed record RetirarCancelamentoVigenciaCommand(Guid PublicId, RetirarCancelamentoVigenciaDados Dados) : IRequest<GestaoAgendamentoResultado>;

public sealed class AlterarCancelamentoVigenciaHandler(IPropostasRepository repository, IOperacaoPropostas operacao,
    IOptions<ConfiguracaoPropostas> configuracao, TimeProvider relogio)
    : IRequestHandler<AlterarCancelamentoVigenciaCommand, CancelamentoVigenciaResultado>
{
    public Task<CancelamentoVigenciaResultado> Handle(AlterarCancelamentoVigenciaCommand request, CancellationToken cancellationToken) =>
        operacao.ExecutarAsync("Alterar Pedido de Cancelamento de Vigência", async ct =>
        {
            var p = await repository.ObterParaPreenchimentoAsync(request.PublicId, ct) ?? throw new ValidacaoException("Proposta não encontrada.");
            EncerramentoPropostas.ValidarPedido(p, request.Dados.RevisaoEsperada, request.Dados.Observacao);
            EncerramentoPropostas.ValidarPedidoExistente(p);
            await EncerramentoPropostas.ValidarVigenteAsync(p, repository, configuracao.Value, ct);
            var hoje = VigenciaPropostas.Hoje(relogio);
            var data = EncerramentoPropostas.ValidarDataCancelamento(p, request.Dados.DataCancelamentoVigencia, hoje);
            if (data == p.DataCancelamentoVigencia)
                throw new ValidacaoException("Informe uma data diferente da registrada no pedido de cancelamento.");
            var agendada = data > hoje;
            if (agendada && !configuracao.Value.ProcessarAgendamentos)
                throw new ValidacaoException("O processamento de cancelamentos futuros está desabilitado.");
            var status = await EncerramentoPropostas.ObterStatusAsync(repository, configuracao.Value, false, ct);
            var antes = VigenciaPropostas.Retrato(p); var anterior = p.DataCancelamentoVigencia;
            var agora = relogio.GetUtcNow().UtcDateTime;
            p.DataCancelamentoVigencia = data;
            p.MotivoPendenciaVigencia = null; p.PendenciaVigenciaEm = null;
            if (!agendada) EncerramentoPropostas.Concluir(p, status, agora);
            GestaoAgendamento.Registrar(p, "Alteração do Pedido de Cancelamento de Vigência",
                $"Data Anterior: {anterior:dd/MM/yyyy}. Primeiro Dia sem Vigência: {data:dd/MM/yyyy}. Justificativa: {request.Dados.Observacao!.Trim()}", agora);
            await repository.SalvarAlteracoesAsync(ct);
            return (new CancelamentoVigenciaResultado(!agendada, agendada, p.RevisaoPreenchimento), p.PublicId.ToString(), antes, VigenciaPropostas.Retrato(p));
        }, cancellationToken);
}

public sealed class RetirarCancelamentoVigenciaHandler(IPropostasRepository repository, IOperacaoPropostas operacao,
    IOptions<ConfiguracaoPropostas> configuracao, TimeProvider relogio)
    : IRequestHandler<RetirarCancelamentoVigenciaCommand, GestaoAgendamentoResultado>
{
    public Task<GestaoAgendamentoResultado> Handle(RetirarCancelamentoVigenciaCommand request, CancellationToken cancellationToken) =>
        operacao.ExecutarAsync("Retirar Pedido de Cancelamento de Vigência", async ct =>
        {
            var p = await repository.ObterParaPreenchimentoAsync(request.PublicId, ct) ?? throw new ValidacaoException("Proposta não encontrada.");
            EncerramentoPropostas.ValidarPedido(p, request.Dados.RevisaoEsperada, request.Dados.Observacao);
            EncerramentoPropostas.ValidarPedidoExistente(p);
            await EncerramentoPropostas.ValidarVigenteAsync(p, repository, configuracao.Value, ct);
            var antes = VigenciaPropostas.Retrato(p); var anterior = p.DataCancelamentoVigencia;
            var agora = relogio.GetUtcNow().UtcDateTime;
            p.DataCancelamentoVigencia = null; p.CancelamentoSolicitadoEm = null;
            p.MotivoPendenciaVigencia = null; p.PendenciaVigenciaEm = null;
            GestaoAgendamento.Registrar(p, "Retirada do Pedido de Cancelamento de Vigência",
                $"Data Retirada: {anterior:dd/MM/yyyy}. Justificativa: {request.Dados.Observacao!.Trim()}", agora);
            await repository.SalvarAlteracoesAsync(ct);
            return (new GestaoAgendamentoResultado(p.RevisaoPreenchimento), p.PublicId.ToString(), antes, VigenciaPropostas.Retrato(p));
        }, cancellationToken);
}
