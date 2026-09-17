using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarModulos;

public class ObterModuloApolicePorPublicIdHandler : IRequestHandler<ObterModuloApolicePorPublicIdQuery, ModuloApoliceDto>
{
    private readonly SeguroDbContext _dbContext;

    public ObterModuloApolicePorPublicIdHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ModuloApoliceDto> Handle(ObterModuloApolicePorPublicIdQuery request, CancellationToken cancellationToken)
    {
        var apolice = await _dbContext.Apolices
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.PublicId == request.ApolicePublicId && a.DeletedAt == null, cancellationToken);

        if (apolice == null)
            throw new ValidacaoException("Apólice não encontrada.");

        var vinculo = await _dbContext.ApoliceModulos
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.PublicId == request.ApoliceModuloPublicId && m.DeletedAt == null, cancellationToken);

        if (vinculo == null)
            throw new ValidacaoException("Vínculo do Módulo não encontrado.");

        if (vinculo.ApoliceId != apolice.Id)
            throw new ValidacaoException("O Vínculo informado não pertence à Apólice especificada.");

        var moduloGlobal = await _dbContext.Database
            .SqlQuery<ModuloGlobalDto>($"SELECT id AS \"Id\", public_id AS \"PublicId\", nome AS \"Nome\", descricao AS \"Descricao\", ativo AS \"Ativo\" FROM cadastro.modulo WHERE id = {vinculo.ModuloId}")
            .FirstOrDefaultAsync(cancellationToken);

        if (moduloGlobal == null)
            throw new ValidacaoException("Módulo Global associado não encontrado.");

        return new ModuloApoliceDto
        {
            PublicId = vinculo.PublicId,
            ModuloPublicId = moduloGlobal.PublicId,
            Nome = moduloGlobal.Nome,
            Descricao = moduloGlobal.Descricao,
            DataInicio = vinculo.DataInicio,
            DataFim = vinculo.DataFim,
            Observacao = vinculo.Observacao,
            Ativo = vinculo.Ativo
        };
    }
}
