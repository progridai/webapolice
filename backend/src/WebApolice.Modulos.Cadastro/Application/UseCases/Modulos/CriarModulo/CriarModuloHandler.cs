using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;
using WebApolice.SharedKernel.Application.Security;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.CriarModulo;

public class CriarModuloHandler : IRequestHandler<CriarModuloCommand, ModuloDto>
{
    private readonly CadastroDbContext _dbContext;
    public CriarModuloHandler(CadastroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ModuloDto> Handle(CriarModuloCommand request, CancellationToken cancellationToken)
    {
        // Regra de negócio: Nome do módulo é único
        var existe = await _dbContext.Modulos.AnyAsync(m => m.Nome.ToLower() == request.Nome.ToLower() && m.DeletedAt == null, cancellationToken);
        if (existe)
        {
            throw new ValidacaoException("Já existe um Módulo com este nome no catálogo global.");
        }

        var modulo = new ModuloModel
        {
            PublicId = Guid.NewGuid(),
            Nome = request.Nome,
            Descricao = request.Descricao,
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Modulos.Add(modulo);
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
