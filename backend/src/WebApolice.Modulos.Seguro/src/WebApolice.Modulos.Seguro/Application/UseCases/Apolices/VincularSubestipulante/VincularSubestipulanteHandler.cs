using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.VincularSubestipulante;

public class VincularSubestipulanteHandler : IRequestHandler<VincularSubestipulanteCommand, long>
{
    private readonly IApoliceVinculosRepository _repository;

    public VincularSubestipulanteHandler(IApoliceVinculosRepository dbContext)
    {
        _repository = dbContext;
    }

    public async Task<long> Handle(VincularSubestipulanteCommand request, CancellationToken cancellationToken)
    {
        var apoliceExiste = await _repository.ApoliceExisteAsync(request.ApoliceId, cancellationToken);
        if (!apoliceExiste) throw new ValidacaoException("Apólice não encontrada.");

        var vinculoExistente = await _repository.ObterVinculoSubestipulanteAsync(request.ApoliceId, request.SubestipulanteId, cancellationToken);
        
        if (vinculoExistente != null && vinculoExistente.Ativo)
        {
            throw new ValidacaoException("O Subestipulante já está vinculado ativamente nesta Apólice.");
        }

        ApoliceSubestipulanteModel subestipulanteVinculo;
        
        if (vinculoExistente != null && !vinculoExistente.Ativo)
        {
            // Reativação
            vinculoExistente.Ativo = true;
            vinculoExistente.UpdatedAt = DateTimeOffset.UtcNow;
            subestipulanteVinculo = vinculoExistente;
            _repository.AtualizarSubestipulante(vinculoExistente);
        }
        else
        {
            // Novo
            subestipulanteVinculo = new ApoliceSubestipulanteModel
            {
                ApoliceId = request.ApoliceId,
                SubestipulanteId = request.SubestipulanteId,
                Ativo = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _repository.AdicionarSubestipulante(subestipulanteVinculo);
        }

        // Adiciona rastro no Histórico
        var historico = new ApoliceHistoricoModel
        {
            ApoliceId = request.ApoliceId,
            Acao = "Vínculo Subestipulante",
            Descricao = $"Subestipulante ID {request.SubestipulanteId} foi vinculado à apólice.",
            UsuarioPublicId = request.UsuarioPublicId,
            DataAcao = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _repository.AdicionarHistorico(historico);

        await _repository.SalvarAlteracoesAsync(cancellationToken);

        return subestipulanteVinculo.Id;
    }
}
