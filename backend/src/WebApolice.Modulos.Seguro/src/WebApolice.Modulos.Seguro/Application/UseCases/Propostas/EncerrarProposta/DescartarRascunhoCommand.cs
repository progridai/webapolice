using MediatR;
using Microsoft.Extensions.Options;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.GerirAgendamento;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.EncerrarProposta;

public sealed record DescartarRascunhoDados(int? RevisaoEsperada, string? Observacao);
public sealed record DescartarRascunhoCommand(Guid PublicId, DescartarRascunhoDados Dados) : IRequest<GestaoAgendamentoResultado>;

public sealed class DescartarRascunhoHandler(IPropostasRepository repository, IOperacaoPropostas operacao,
    IOptions<ConfiguracaoPropostas> configuracao, TimeProvider relogio) : IRequestHandler<DescartarRascunhoCommand, GestaoAgendamentoResultado>
{
    public Task<GestaoAgendamentoResultado> Handle(DescartarRascunhoCommand request, CancellationToken cancellationToken) =>
        operacao.ExecutarAsync("Descartar Rascunho de Proposta", async ct =>
        {
            var p = await repository.ObterParaPreenchimentoAsync(request.PublicId, ct) ?? throw new ValidacaoException("Proposta não encontrada.");
            EncerramentoPropostas.ValidarPedido(p, request.Dados.RevisaoEsperada, request.Dados.Observacao);
            if (!p.PermitePreenchimento(configuracao.Value.StatusInicialCodigo))
                throw new ValidacaoException("Somente um rascunho nativo ainda sem confirmação e sem vigência pode ser descartado.");
            var status = await EncerramentoPropostas.ObterStatusAsync(repository, configuracao.Value, true, ct);
            var antes = VigenciaPropostas.Retrato(p); var agora = relogio.GetUtcNow().UtcDateTime;
            p.StatusId = status.Id; p.Status = status; p.DescartadaEm = agora;
            GestaoAgendamento.Registrar(p, "Descarte de Rascunho", request.Dados.Observacao!.Trim(), agora);
            await repository.SalvarAlteracoesAsync(ct);
            return (new GestaoAgendamentoResultado(p.RevisaoPreenchimento), p.PublicId.ToString(), antes, VigenciaPropostas.Retrato(p));
        }, cancellationToken);
}
