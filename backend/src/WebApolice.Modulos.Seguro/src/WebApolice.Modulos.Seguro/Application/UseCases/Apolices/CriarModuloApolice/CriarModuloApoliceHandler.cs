using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.CriarModuloApolice;

public class CriarModuloApoliceHandler : IRequestHandler<CriarModuloApoliceCommand, Guid>
{
    private readonly SeguroDbContext _dbContext;

    public CriarModuloApoliceHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CriarModuloApoliceCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar vigência
        if (request.DataFim.HasValue && request.DataInicio.HasValue && request.DataFim < request.DataInicio)
        {
            throw new ValidacaoException("A data de fim de vigência não pode ser anterior à data de início.");
        }

        // 2. Localizar Apólice
        var apolice = await _dbContext.Apolices
            .FirstOrDefaultAsync(a => a.PublicId == request.ApolicePublicId && a.DeletedAt == null, cancellationToken);
            
        if (apolice == null)
            throw new ValidacaoException("Apólice não encontrada.");
            
        // Validar limite de vigência da Apólice (regra de negócio: módulo deve estar contido na vigência da apólice, caso ela tenha datas)
        // Convertendo DateTime? para DateOnly? se for necessário comparar com Apolice (Apolice tem DateOnly)
        // DateOnly? dataInicioApolice = apolice.DataInicioVigencia; // Isso não é anulável em ApoliceModel
        var dataInicioReqDate = request.DataInicio.HasValue ? DateOnly.FromDateTime(request.DataInicio.Value) : (DateOnly?)null;
        var dataFimReqDate = request.DataFim.HasValue ? DateOnly.FromDateTime(request.DataFim.Value) : (DateOnly?)null;
        
        if (dataInicioReqDate.HasValue && dataInicioReqDate < apolice.DataInicioVigencia)
            throw new ValidacaoException($"A data de início do Módulo não pode ser anterior à data de início de vigência da Apólice ({apolice.DataInicioVigencia}).");

        if (apolice.DataFimVigencia.HasValue && dataFimReqDate.HasValue && dataFimReqDate > apolice.DataFimVigencia)
            throw new ValidacaoException($"A data de fim do Módulo não pode ser posterior à data de fim de vigência da Apólice ({apolice.DataFimVigencia}).");

        // 3. Resolver Módulo Global via SQL (cross-module)
        var modulo = await _dbContext.Database
            .SqlQuery<ModuloGlobalDto>($"SELECT id, nome, descricao, ativo FROM cadastro.modulo WHERE public_id = {request.ModuloPublicId} AND deleted_at IS NULL")
            .FirstOrDefaultAsync(cancellationToken);

        if (modulo == null)
            throw new ValidacaoException("Módulo não encontrado no Cadastro Global.");

        if (!modulo.Ativo)
            throw new ValidacaoException("O Módulo Global está inativo. Não é possível vincular um Módulo inativo.");

        // 4. Verificar duplicidade (inclui inativos não deletados)
        var vinculoExistente = await _dbContext.ApoliceModulos
            .FirstOrDefaultAsync(m =>
                m.ApoliceId == apolice.Id &&
                m.ModuloId == modulo.Id &&
                m.DeletedAt == null, cancellationToken);

        if (vinculoExistente != null)
        {
            if (vinculoExistente.Ativo)
                throw new ValidacaoException("Este Módulo já está vinculado ativamente a esta Apólice.");
            else
                throw new ValidacaoException($"Já existe um vínculo inativo do Módulo '{modulo.Nome}' com esta Apólice. A reativação silenciosa não é permitida.");
        }

        // 5. Criar Vínculo
        var novoVinculo = new ApoliceModuloModel
        {
            ApoliceId = apolice.Id,
            ModuloId = modulo.Id,
            DataInicio = request.DataInicio,
            DataFim = request.DataFim,
            Observacao = request.Observacao,
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        _dbContext.ApoliceModulos.Add(novoVinculo);

        // 6. Registrar Histórico Funcional
        _dbContext.ApoliceHistoricos.Add(new ApoliceHistoricoModel
        {
            ApoliceId = apolice.Id,
            Acao = "Vínculo de Módulo",
            Descricao = $"Módulo '{modulo.Nome}' vinculado à Apólice.",
            UsuarioPublicId = request.UsuarioPublicId,
            DataAcao = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return novoVinculo.PublicId;
    }
}

/// <summary>DTO interno para leitura cross-module do Cadastro Global de Módulos.</summary>
internal class ModuloGlobalDto
{
    public long Id { get; set; }
    public string Nome { get; set; } = null!;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; }
}
