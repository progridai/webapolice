using MediatR;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.CriarProposta;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ReincluirProposta;

public sealed record PrepararReinclusaoDados(int? RevisaoEsperada, string? OrigemCondicoes);
public sealed record PrepararReinclusaoCommand(Guid PublicId, PrepararReinclusaoDados Dados) : IRequest<Guid>;

public sealed class PrepararReinclusaoHandler(IPropostasRepository repository, IMediator mediator)
    : IRequestHandler<PrepararReinclusaoCommand, Guid>
{
    public async Task<Guid> Handle(PrepararReinclusaoCommand request, CancellationToken ct)
    {
        if (request.PublicId == Guid.Empty || request.Dados.RevisaoEsperada is null or < 0 || request.Dados.OrigemCondicoes is not ("ANTERIOR" or "MODULO"))
            throw new ValidacaoException("Informe a revisão e escolha as condições da proposta anterior ou do Módulo.");
        var origem = await repository.ObterDetalheAsync(request.PublicId, ct)
            ?? throw new ValidacaoException("Proposta de origem não encontrada.");
        if (origem.ApolicePublicId is not Guid apolice || origem.ApoliceVidaPublicId is not Guid vidaId)
            throw new ValidacaoException("A origem deve pertencer a uma Apólice e Vida.");
        var vida = await repository.ObterVidaAsync(vidaId, ct) ?? throw new ValidacaoException("Vida não encontrada.");
        // A criação reobtém e valida a origem/revisão após bloquear a mesma Apólice.
        return await mediator.Send(new CriarPropostaCommand
        {
            ApoliceId = apolice, ApoliceVidaId = vidaId, ClienteId = vida.ClienteId,
            PropostaOrigemPublicId = request.PublicId, OrigemCondicoesReinclusao = request.Dados.OrigemCondicoes,
            RevisaoOrigemEsperada = request.Dados.RevisaoEsperada
        }, ct);
    }
}
