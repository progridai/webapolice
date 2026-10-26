using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AtualizarRamo;

public class AtualizarRamoApoliceHandler : IRequestHandler<AtualizarRamoApoliceCommand, bool>
{
    private readonly IApoliceVinculosRepository _repository;

    public AtualizarRamoApoliceHandler(IApoliceVinculosRepository dbContext)
    {
        _repository = dbContext;
    }

    public async Task<bool> Handle(AtualizarRamoApoliceCommand request, CancellationToken cancellationToken)
    {
        var apolice = await _repository.ObterApoliceAsync(request.ApolicePublicId, true, cancellationToken);
        if (apolice == null) throw new ValidacaoException("Apólice não encontrada.");

        var ramo = await _repository.ObterRamoAsync(request.RamoPublicId, cancellationToken);
        if (ramo == null) throw new ValidacaoException("Ramo não encontrado.");

        var vinculo = await _repository.ObterVinculoRamoAsync(apolice.Id, ramo.Id, cancellationToken);

        if (vinculo == null || !vinculo.Ativo)
        {
            throw new ValidacaoException("Vínculo de Ramo ativo não encontrado para esta apólice.");
        }

        vinculo.NumeroApolice = request.NumeroApolice;
        vinculo.IofPercentual = request.IofPercentual;
        vinculo.UpdatedAt = DateTimeOffset.UtcNow;

        _repository.AtualizarRamo(vinculo);

        // WORKAROUND: Tabela seguro.apolice_historico não existe no banco de dados atual
        // var historico = new ApoliceHistoricoModel
        // {
        //     ApoliceId = apolice.Id,
        //     Acao = "Atualização de Vínculo de Ramo",
        //     Descricao = $"Vínculo do Ramo {ramo.Codigo} ({ramo.Nome}) foi atualizado na apólice.",
        //     UsuarioPublicId = request.UsuarioPublicId,
        //     DataAcao = DateTimeOffset.UtcNow,
        //     CreatedAt = DateTimeOffset.UtcNow
        // };
        // _repository.AdicionarHistorico(historico);

        await _repository.SalvarAlteracoesAsync(cancellationToken);

        return true;
    }
}
