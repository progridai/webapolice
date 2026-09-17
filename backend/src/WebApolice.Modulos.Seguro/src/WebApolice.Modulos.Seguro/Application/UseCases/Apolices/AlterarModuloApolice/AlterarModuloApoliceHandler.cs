using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AlterarModuloApolice;

public class AlterarModuloApoliceHandler : IRequestHandler<AlterarModuloApoliceCommand, Unit>
{
    private readonly SeguroDbContext _dbContext;

    public AlterarModuloApoliceHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(AlterarModuloApoliceCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar vigência local
        if (request.DataFim.HasValue && request.DataInicio.HasValue && request.DataFim < request.DataInicio)
        {
            throw new ValidacaoException("A data de fim de vigência não pode ser anterior à data de início.");
        }

        // 2. Localizar Apólice
        var apolice = await _dbContext.Apolices
            .FirstOrDefaultAsync(a => a.PublicId == request.ApolicePublicId && a.DeletedAt == null, cancellationToken);
            
        if (apolice == null)
            throw new ValidacaoException("Apólice não encontrada.");
            
        var dataInicioReqDate = request.DataInicio.HasValue ? DateOnly.FromDateTime(request.DataInicio.Value) : (DateOnly?)null;
        var dataFimReqDate = request.DataFim.HasValue ? DateOnly.FromDateTime(request.DataFim.Value) : (DateOnly?)null;
        
        if (dataInicioReqDate.HasValue && dataInicioReqDate < apolice.DataInicioVigencia)
            throw new ValidacaoException($"A data de início do Módulo não pode ser anterior à data de início de vigência da Apólice ({apolice.DataInicioVigencia}).");

        if (apolice.DataFimVigencia.HasValue && dataFimReqDate.HasValue && dataFimReqDate > apolice.DataFimVigencia)
            throw new ValidacaoException($"A data de fim do Módulo não pode ser posterior à data de fim de vigência da Apólice ({apolice.DataFimVigencia}).");

        // 3. Localizar Vínculo e garantir isolamento
        var vinculo = await _dbContext.ApoliceModulos
            .FirstOrDefaultAsync(m => m.PublicId == request.ApoliceModuloPublicId && m.DeletedAt == null, cancellationToken);

        if (vinculo == null)
            throw new ValidacaoException("Vínculo do Módulo não encontrado.");

        if (vinculo.ApoliceId != apolice.Id)
            throw new ValidacaoException("O Vínculo informado não pertence à Apólice especificada.");

        if (!vinculo.Ativo)
            throw new ValidacaoException("O Vínculo do Módulo está inativo e não pode ser alterado.");

        // 4. Alterar Vínculo
        vinculo.DataInicio = request.DataInicio;
        vinculo.DataFim = request.DataFim;
        vinculo.Observacao = request.Observacao;
        vinculo.UpdatedAt = DateTimeOffset.UtcNow;

        // 5. Histórico
        // Buscar nome do Módulo para compor o histórico
        var moduloNome = await _dbContext.Database
            .SqlQuery<string>($"SELECT nome AS \"Value\" FROM cadastro.modulo WHERE id = {vinculo.ModuloId}")
            .FirstOrDefaultAsync(cancellationToken);

        _dbContext.ApoliceHistoricos.Add(new ApoliceHistoricoModel
        {
            ApoliceId = apolice.Id,
            Acao = "Alteração de Vínculo de Módulo",
            Descricao = $"Vínculo do Módulo '{moduloNome}' alterado na Apólice.",
            UsuarioPublicId = request.UsuarioPublicId,
            DataAcao = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
