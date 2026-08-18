using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Exceptions;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.AtualizarModulo;

public class AtualizarModuloHandler : IRequestHandler<AtualizarModuloCommand, ModuloDto>
{
    private readonly CadastroDbContext _dbContext;

    public AtualizarModuloHandler(CadastroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ModuloDto> Handle(AtualizarModuloCommand request, CancellationToken cancellationToken)
    {
        var modulo = await _dbContext.Modulos.FirstOrDefaultAsync(m => m.PublicId == request.PublicId && m.DeletedAt == null, cancellationToken);
        if (modulo == null)
        {
            throw new ValidacaoException("Módulo não encontrado no catálogo.");
        }

        var existeOutroComNome = await _dbContext.Modulos.AnyAsync(m => m.Id != modulo.Id && m.Nome.ToLower() == request.Nome.ToLower() && m.DeletedAt == null, cancellationToken);
        if (existeOutroComNome)
        {
            throw new ValidacaoException("Já existe outro Módulo com este nome no catálogo.");
        }

        modulo.Nome = request.Nome;
        modulo.Descricao = request.Descricao;
        modulo.Ativo = request.Ativo;
        modulo.UpdatedAt = DateTimeOffset.UtcNow;

        _dbContext.Modulos.Update(modulo);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ModuloDto(
            modulo.PublicId,
            modulo.Nome,
            modulo.Descricao,
            modulo.Ativo,
            modulo.CreatedAt,
            modulo.UpdatedAt
        );
    }
}
