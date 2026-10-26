using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.VincularRamo;

public class VincularRamoApoliceHandler : IRequestHandler<VincularRamoApoliceCommand, long>
{
    private readonly IApoliceVinculosRepository _repository;

    public VincularRamoApoliceHandler(IApoliceVinculosRepository dbContext)
    {
        _repository = dbContext;
    }

    public async Task<long> Handle(VincularRamoApoliceCommand request, CancellationToken cancellationToken)
    {
        var apolice = await _repository.ObterApoliceAsync(request.ApolicePublicId, true, cancellationToken);
        if (apolice == null) throw new ValidacaoException("Apólice não encontrada.");

        var ramo = await _repository.ObterRamoAsync(request.RamoPublicId, cancellationToken);
        if (ramo == null) throw new ValidacaoException("Ramo não encontrado.");
        if (!ramo.Ativo) throw new ValidacaoException("Não é possível vincular um Ramo inativo.");

        var vinculoExistente = await _repository.ObterVinculoRamoAsync(apolice.Id, ramo.Id, cancellationToken);
        
        if (vinculoExistente != null && vinculoExistente.Ativo)
        {
            throw new ValidacaoException("O Ramo já está vinculado ativamente nesta Apólice.");
        }

        if (vinculoExistente != null && !vinculoExistente.Ativo)
        {
            throw new ValidacaoException("Existe um vínculo inativo com este Ramo. A reativação de vínculos ainda não está implementada/aprovada.");
        }

        // Novo
        var ramoVinculo = new ApoliceRamoModel
        {
            ApoliceId = apolice.Id,
            RamoId = ramo.Id,
            NumeroApolice = request.NumeroApolice,
            IofPercentual = request.IofPercentual,
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _repository.AdicionarRamo(ramoVinculo);

        var historico = new ApoliceHistoricoModel
        {
            ApoliceId = apolice.Id,
            Acao = "Vínculo de Ramo",
            Descricao = $"Ramo {ramo.Codigo} ({ramo.Nome}) foi vinculado à apólice.",
            UsuarioPublicId = request.UsuarioPublicId,
            DataAcao = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _repository.AdicionarHistorico(historico);


        await _repository.SalvarAlteracoesAsync(cancellationToken);

        return ramoVinculo.Id;
    }
}
