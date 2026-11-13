using MediatR;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.CriarProposta;

public sealed record CriarPropostaDaVidaCommand(Guid ApolicePublicId, Guid VidaPublicId, Guid? PropostaOrigemPublicId) : IRequest<Guid>;

public sealed class CriarPropostaDaVidaHandler(IPropostasRepository repository, IMediator mediator) : IRequestHandler<CriarPropostaDaVidaCommand, Guid>
{
    public async Task<Guid> Handle(CriarPropostaDaVidaCommand request, CancellationToken cancellationToken)
    {
        var vida = await repository.ObterVidaAsync(request.VidaPublicId, cancellationToken)
            ?? throw new ValidacaoException("Vida não encontrada.");
        return await mediator.Send(new CriarPropostaCommand
        {
            ApoliceId = request.ApolicePublicId, ApoliceVidaId = request.VidaPublicId,
            ClienteId = vida.ClienteId, PropostaOrigemPublicId = request.PropostaOrigemPublicId
        }, cancellationToken);
    }
}
