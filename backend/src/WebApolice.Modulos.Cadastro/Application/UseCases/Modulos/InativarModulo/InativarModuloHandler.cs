using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.InativarModulo;

public class InativarModuloHandler : IRequestHandler<InativarModuloCommand, bool>
{
    private readonly CadastroDbContext _dbContext;

    public InativarModuloHandler(CadastroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(InativarModuloCommand request, CancellationToken cancellationToken)
    {
        var modulo = await _dbContext.Modulos.FirstOrDefaultAsync(m => m.PublicId == request.PublicId && m.DeletedAt == null, cancellationToken);
        if (modulo == null)
        {
            throw new ValidacaoException("Módulo não encontrado no catálogo.");
        }

        // Exclusão Lógica
        modulo.DeletedAt = DateTimeOffset.UtcNow;
        modulo.Ativo = false;
        modulo.UpdatedAt = DateTimeOffset.UtcNow;

        _dbContext.Modulos.Update(modulo);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
