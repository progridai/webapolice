using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.VincularRamo;

public class VincularRamoApoliceHandler : IRequestHandler<VincularRamoApoliceCommand, long>
{
    private readonly SeguroDbContext _dbContext;

    public VincularRamoApoliceHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<long> Handle(VincularRamoApoliceCommand request, CancellationToken cancellationToken)
    {
        var apolice = await _dbContext.Apolices.FirstOrDefaultAsync(a => a.PublicId == request.ApolicePublicId, cancellationToken);
        if (apolice == null) throw new ValidacaoException("Apólice não encontrada.");

        var ramo = await _dbContext.Ramos.FirstOrDefaultAsync(r => r.PublicId == request.RamoPublicId, cancellationToken);
        if (ramo == null) throw new ValidacaoException("Ramo não encontrado.");
        if (!ramo.Ativo) throw new ValidacaoException("Não é possível vincular um Ramo inativo.");

        var vinculoExistente = await _dbContext.ApoliceRamos
            .FirstOrDefaultAsync(ar => ar.ApoliceId == apolice.Id && ar.RamoId == ramo.Id, cancellationToken);
        
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
        _dbContext.ApoliceRamos.Add(ramoVinculo);

        // WORKAROUND: Tabela seguro.apolice_historico não existe no banco de dados atual
        // var historico = new ApoliceHistoricoModel
        // {
        //     ApoliceId = apolice.Id,
        //     Acao = "Vínculo de Ramo",
        //     Descricao = $"Ramo {ramo.Codigo} ({ramo.Nome}) foi vinculado à apólice.",
        //     UsuarioPublicId = request.UsuarioPublicId,
        //     DataAcao = DateTimeOffset.UtcNow,
        //     CreatedAt = DateTimeOffset.UtcNow
        // };
        // _dbContext.ApoliceHistoricos.Add(historico);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ramoVinculo.Id;
    }
}
