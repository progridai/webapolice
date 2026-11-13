using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ExcluirApoliceVida;

public sealed record ExcluirApoliceVidaCommand(Guid ApolicePublicId, Guid ApoliceVidaPublicId, Guid UsuarioPublicId);

public sealed class ExcluirApoliceVidaHandler(IApoliceVinculosRepository repository, IOperacaoVidas operacao)
{
    public async Task Handle(ExcluirApoliceVidaCommand request, CancellationToken cancellationToken)
    {
        await operacao.ExecutarAsync<bool>("VIDA_EXCLUIDA", async ct =>
        {
            var apolice = await repository.ObterApoliceAsync(request.ApolicePublicId, false, ct)
                ?? throw new ValidacaoException("Apólice não encontrada.");
            var vida = await repository.ObterVidaParaExclusaoAsync(request.ApoliceVidaPublicId, apolice.Id, ct)
                ?? throw new ValidacaoException("Participação não encontrada nesta apólice ou já excluída.");
            if (await repository.VidaPossuiPropostasAsync(vida.Id, ct))
                throw new ValidacaoException("Esta participação possui propostas e não pode ser excluída. Utilize Encerrar para finalizar a participação.");

            var antes = new { vida.PublicId, vida.Ativo, vida.DeletedAt, vida.UpdatedAt };
            var agora = DateTimeOffset.UtcNow;
            vida.DeletedAt = agora;
            vida.UpdatedAt = agora;
            vida.Ativo = false;
            repository.AdicionarHistorico(new ApoliceHistoricoModel
            {
                ApoliceId = apolice.Id,
                Acao = "Exclusão Vida",
                Descricao = $"Participação de Vida (PublicId: {vida.PublicId}) excluída por correção de lançamento.",
                UsuarioPublicId = request.UsuarioPublicId,
                DataAcao = agora,
                CreatedAt = agora
            });
            await repository.SalvarAlteracoesAsync(ct);
            return (true, vida.PublicId.ToString(), antes,
                new { vida.PublicId, vida.Ativo, vida.DeletedAt, vida.UpdatedAt });
        }, cancellationToken);
    }
}
