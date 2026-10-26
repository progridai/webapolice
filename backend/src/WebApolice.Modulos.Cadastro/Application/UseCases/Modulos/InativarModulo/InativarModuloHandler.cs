using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;
using WebApolice.Modulos.Cadastro.Application.Ports;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.InativarModulo;

public class InativarModuloHandler : IRequestHandler<InativarModuloCommand, bool>
{
    private readonly IModulosCadastroRepository _dbContext;

    public InativarModuloHandler(IModulosCadastroRepository dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(InativarModuloCommand request, CancellationToken cancellationToken)
    {
        var modulo = await _dbContext.ObterAsync(request.PublicId, cancellationToken);
        if (modulo == null)
        {
            throw new ModuloNaoEncontradoException("Módulo não encontrado no catálogo.");
        }

        // Exclusão Lógica
        modulo.DeletedAt = DateTimeOffset.UtcNow;
        modulo.Ativo = false;
        modulo.UpdatedAt = DateTimeOffset.UtcNow;

        _dbContext.Atualizar(modulo);
        await _dbContext.SalvarAlteracoesAsync(cancellationToken);

        return true;
    }
}
