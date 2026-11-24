using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.Financeiro;
using MediatR;
using Microsoft.Extensions.Options;
using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ConfirmarProposta;

public sealed record ConfirmarPropostaDados(int? RevisaoEsperada, DateOnly? DataInicioVigencia, FinanceiroConfirmacaoDados? Financeiro = null);
public sealed record ConfirmarPropostaCommand(Guid PublicId, ConfirmarPropostaDados Dados) : IRequest<ConfirmarPropostaResultado>;
public sealed record ConfirmarPropostaResultado(bool Vigente, bool Agendada, int RevisaoPreenchimento);

public sealed class ConfirmarPropostaHandler(IPropostasRepository repository, IOperacaoPropostas operacao,
    IClientesPropostas clientes, ICadastroSeguroQueries cadastro, IOptions<ConfiguracaoPropostas> configuracao, TimeProvider relogio, IParcelasPropostasRepository? financeiro = null)
    : IRequestHandler<ConfirmarPropostaCommand, ConfirmarPropostaResultado>
{
    public Task<ConfirmarPropostaResultado> Handle(ConfirmarPropostaCommand request, CancellationToken cancellationToken) =>
        operacao.ExecutarAsync("Confirmar Proposta", async ct =>
        {
            if (request.PublicId == Guid.Empty || request.Dados.RevisaoEsperada is null or < 0 || request.Dados.DataInicioVigencia is null)
                throw new ValidacaoException("Informe a revisão e a data de início da vigência.");
            var p = await repository.ObterParaPreenchimentoAsync(request.PublicId, ct) ?? throw new ValidacaoException("Proposta não encontrada.");
            if (p.ConfirmadaEm.HasValue) throw new ValidacaoException("Esta proposta já foi confirmada. Recarregue a consulta para verificar sua vigência.");
            if (!p.PermitePreenchimento(configuracao.Value.StatusInicialCodigo)) throw new ValidacaoException("Somente uma pré proposta nativa pode ser confirmada.");
            if (p.IncluiConjuge is null) throw new ValidacaoException("Informe explicitamente se a proposta inclui cônjuge antes de confirmar.");
            if (p.RevisaoPreenchimento != request.Dados.RevisaoEsperada) throw new ValidacaoException("A proposta foi alterada em outra tela. Recarregue antes de confirmar.");
            var codigo = configuracao.Value.StatusConfirmadaCodigo;
            if (string.IsNullOrWhiteSpace(codigo) || codigo == configuracao.Value.StatusInicialCodigo)
                throw new ValidacaoException("A situação de confirmação ainda não foi configurada corretamente.");
            var status = await repository.ObterStatusAsync(codigo, ct);
            if (status is null || !status.VisivelOperacional || status.Finalizador || status.PermiteMovimentacao)
                throw new ValidacaoException("A situação de confirmação deve ser visível, não finalizadora e sem movimentação financeira.");
            await VigenciaPropostas.ValidarContextoAsync(p, repository, clientes, cadastro, ct);
            var inicio = request.Dados.DataInicioVigencia.Value; var hoje = VigenciaPropostas.Hoje(relogio);
            var agendada = inicio > hoje;
            if (agendada && !configuracao.Value.ProcessarAgendamentos)
                throw new ValidacaoException("A confirmação com início futuro está temporariamente indisponível.");
            VigenciaPropostas.ValidarDatas(p, inicio, hoje, efetivar: !agendada);
            var origem = await VigenciaPropostas.ValidarOrigemAsync(p, inicio, repository, configuracao.Value, ct);
            var antes = VigenciaPropostas.Retrato(p, origem); var agora = relogio.GetUtcNow().UtcDateTime;
            if (configuracao.Value.ExigirConferenciaFinanceira || request.Dados.Financeiro != null)
            {
                if (financeiro == null || request.Dados.Financeiro == null)
                    throw new ValidacaoException("Confira a competência, o vencimento e os valores da primeira parcela antes de confirmar.");
                await FinanceiroConferencia.SalvarAsync(p, inicio, request.Dados.Financeiro, repository, financeiro, relogio, ct);
            }
            p.StatusId = status.Id; p.Status = status; p.ConfirmadaEm = agora; p.DataInicioVigencia = inicio;
            p.RevisaoPreenchimento = checked(p.RevisaoPreenchimento + 1); p.UpdatedAt = agora;
            if (agendada && origem is not null) VigenciaPropostas.Historico(p, origem,
                origem.CanceladaEm != null && origem.DataFimVigencia != null ? "Reinclusão Agendada" : "Substituição Agendada", agora);
            if (request.Dados.Financeiro != null) await repository.SalvarAlteracoesAsync(ct);
            if (!agendada) await VigenciaPropostas.EfetivarAsync(p, origem, agora, repository, ct, financeiro);
            await repository.SalvarAlteracoesAsync(ct);
            return (new ConfirmarPropostaResultado(p.Vigente, agendada, p.RevisaoPreenchimento), p.PublicId.ToString(), antes, VigenciaPropostas.Retrato(p, origem));
        }, cancellationToken);
}
