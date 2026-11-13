using MediatR;
using Microsoft.Extensions.Options;
using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.GerirAgendamento;

public sealed record ReagendarPropostaDados(int? RevisaoEsperada, DateOnly? DataInicioVigencia, string? Observacao);
public sealed record CancelarAgendamentoDados(int? RevisaoEsperada, string? Observacao);
public sealed record ReagendarPropostaCommand(Guid PublicId, ReagendarPropostaDados Dados) : IRequest<GestaoAgendamentoResultado>;
public sealed record CancelarAgendamentoCommand(Guid PublicId, CancelarAgendamentoDados Dados) : IRequest<GestaoAgendamentoResultado>;
public sealed record GestaoAgendamentoResultado(int RevisaoPreenchimento);

internal static class GestaoAgendamento
{
    internal static void Validar(Propostum p, int? revisao, string? observacao, ConfiguracaoPropostas config)
    {
        if (revisao is null or < 0 || string.IsNullOrWhiteSpace(observacao) || observacao.Length > 1000)
            throw new ValidacaoException("Informe a revisão e uma justificativa de até 1.000 caracteres.");
        if (string.IsNullOrWhiteSpace(config.StatusConfirmadaCodigo) || p.Status.Codigo != config.StatusConfirmadaCodigo ||
            p.DescartadaEm != null || p.CancelamentoSolicitadoEm != null || p.LegadoId != null || p.ConfirmadaEm == null || p.DataInicioVigencia == null || p.CanceladaEm != null ||
            p.Vigente || p.SubstituidaEm != null || p.DataFimVigencia != null || p.DeletedAt != null || !p.VisivelOperacional ||
            !p.Status.VisivelOperacional || p.Status.Finalizador || p.Status.PermiteMovimentacao || p.ApoliceId == null || p.ApoliceVidaId == null)
            throw new ValidacaoException("Somente uma proposta nativa confirmada e ainda sem vigência pode ter seu agendamento alterado.");
        if (p.RevisaoPreenchimento != revisao)
            throw new ValidacaoException("A proposta foi alterada em outra tela ou pelo processamento da vigência. Recarregue antes de continuar.");
    }

    internal static void Registrar(Propostum p, string motivo, string observacao, DateTime agora)
    {
        p.PropostaHistoricoPropostaNovas.Add(new PropostaHistorico
        {
            PropostaAnteriorId = p.PropostaOrigemId ?? p.Id, PropostaNova = p,
            Motivo = motivo, Observacao = observacao, DataAlteracao = agora, CreatedAt = agora
        });
        p.RevisaoPreenchimento = checked(p.RevisaoPreenchimento + 1); p.UpdatedAt = agora;
    }
}

public sealed class ReagendarPropostaHandler(IPropostasRepository repository, IOperacaoPropostas operacao,
    IClientesPropostas clientes, ICadastroSeguroQueries cadastro, IOptions<ConfiguracaoPropostas> configuracao, TimeProvider relogio)
    : IRequestHandler<ReagendarPropostaCommand, GestaoAgendamentoResultado>
{
    public Task<GestaoAgendamentoResultado> Handle(ReagendarPropostaCommand request, CancellationToken cancellationToken) =>
        operacao.ExecutarAsync("Reagendar Proposta", async ct =>
        {
            var p = await repository.ObterParaPreenchimentoAsync(request.PublicId, ct) ?? throw new ValidacaoException("Proposta não encontrada.");
            GestaoAgendamento.Validar(p, request.Dados.RevisaoEsperada, request.Dados.Observacao, configuracao.Value);
            if (!configuracao.Value.ProcessarAgendamentos) throw new ValidacaoException("O processamento dos agendamentos está desabilitado.");
            var inicio = request.Dados.DataInicioVigencia;
            var hoje = VigenciaPropostas.Hoje(relogio);
            if (inicio == null || inicio <= hoje) throw new ValidacaoException("Informe uma nova data de início futura para reagendar.");
            if (inicio == p.DataInicioVigencia) throw new ValidacaoException("Informe uma data diferente do início já agendado.");
            await VigenciaPropostas.ValidarContextoAsync(p, repository, clientes, cadastro, ct);
            VigenciaPropostas.ValidarDatas(p, inicio.Value, hoje, efetivar: false);
            await VigenciaPropostas.ValidarOrigemAsync(p, inicio.Value, repository, ct);
            var antes = VigenciaPropostas.Retrato(p); var agora = relogio.GetUtcNow().UtcDateTime;
            var observacao = $"Início Anterior: {p.DataInicioVigencia:dd/MM/yyyy}. Novo Início: {inicio:dd/MM/yyyy}. Justificativa: {request.Dados.Observacao!.Trim()}";
            p.DataInicioVigencia = inicio; p.MotivoPendenciaVigencia = null; p.PendenciaVigenciaEm = null;
            GestaoAgendamento.Registrar(p, "Reagendamento de Proposta", observacao, agora);
            await repository.SalvarAlteracoesAsync(ct);
            return (new GestaoAgendamentoResultado(p.RevisaoPreenchimento), p.PublicId.ToString(), antes, VigenciaPropostas.Retrato(p));
        }, cancellationToken);
}

public sealed class CancelarAgendamentoHandler(IPropostasRepository repository, IOperacaoPropostas operacao,
    IOptions<ConfiguracaoPropostas> configuracao, TimeProvider relogio)
    : IRequestHandler<CancelarAgendamentoCommand, GestaoAgendamentoResultado>
{
    public Task<GestaoAgendamentoResultado> Handle(CancelarAgendamentoCommand request, CancellationToken cancellationToken) =>
        operacao.ExecutarAsync("Cancelar Agendamento de Proposta", async ct =>
        {
            var p = await repository.ObterParaPreenchimentoAsync(request.PublicId, ct) ?? throw new ValidacaoException("Proposta não encontrada.");
            GestaoAgendamento.Validar(p, request.Dados.RevisaoEsperada, request.Dados.Observacao, configuracao.Value);
            var codigo = configuracao.Value.StatusCanceladaCodigo;
            if (string.IsNullOrWhiteSpace(codigo) || codigo == configuracao.Value.StatusInicialCodigo || codigo == configuracao.Value.StatusConfirmadaCodigo)
                throw new ValidacaoException("A situação de cancelamento ainda não foi configurada corretamente.");
            var status = await repository.ObterStatusAsync(codigo, ct);
            if (status is null || !status.VisivelOperacional || !status.Finalizador || status.PermiteMovimentacao)
                throw new ValidacaoException("A situação de cancelamento deve ser visível, finalizadora e sem movimentação financeira.");
            var antes = VigenciaPropostas.Retrato(p); var agora = relogio.GetUtcNow().UtcDateTime;
            p.CanceladaEm = agora; p.StatusId = status.Id; p.Status = status;
            GestaoAgendamento.Registrar(p, "Cancelamento de Agendamento", request.Dados.Observacao!.Trim(), agora);
            await repository.SalvarAlteracoesAsync(ct);
            return (new GestaoAgendamentoResultado(p.RevisaoPreenchimento), p.PublicId.ToString(), antes, VigenciaPropostas.Retrato(p));
        }, cancellationToken);
}
