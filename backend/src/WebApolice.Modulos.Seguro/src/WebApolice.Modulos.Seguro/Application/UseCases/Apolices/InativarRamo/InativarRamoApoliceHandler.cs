using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarRamo;

public class InativarRamoApoliceHandler : IRequestHandler<InativarRamoApoliceCommand, bool>
{
    private readonly SeguroDbContext _dbContext;

    public InativarRamoApoliceHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(InativarRamoApoliceCommand request, CancellationToken cancellationToken)
    {
        var apolice = await _dbContext.Apolices.FirstOrDefaultAsync(a => a.PublicId == request.ApolicePublicId, cancellationToken);
        if (apolice == null) throw new ValidacaoException("Apólice não encontrada.");

        var ramo = await _dbContext.Ramos.FirstOrDefaultAsync(r => r.PublicId == request.RamoPublicId, cancellationToken);
        if (ramo == null) throw new ValidacaoException("Ramo não encontrado.");

        var vinculo = await _dbContext.ApoliceRamos
            .FirstOrDefaultAsync(ar => ar.ApoliceId == apolice.Id && ar.RamoId == ramo.Id, cancellationToken);

        if (vinculo == null || !vinculo.Ativo)
        {
            throw new ValidacaoException("Vínculo de Ramo não encontrado ou já inativado nesta apólice.");
        }

        vinculo.Ativo = false;
        vinculo.UpdatedAt = DateTimeOffset.UtcNow;

        _dbContext.ApoliceRamos.Update(vinculo);

        // WORKAROUND: Tabela seguro.apolice_historico não existe no banco de dados atual
        // var historico = new ApoliceHistoricoModel
        // {
        //     ApoliceId = apolice.Id,
        //     Acao = "Inativação de Vínculo de Ramo",
        //     Descricao = $"Vínculo do Ramo {ramo.Codigo} ({ramo.Nome}) foi inativado na apólice.",
        //     UsuarioPublicId = request.UsuarioPublicId,
        //     DataAcao = DateTimeOffset.UtcNow,
        //     CreatedAt = DateTimeOffset.UtcNow
        // };
        // _dbContext.ApoliceHistoricos.Add(historico);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
