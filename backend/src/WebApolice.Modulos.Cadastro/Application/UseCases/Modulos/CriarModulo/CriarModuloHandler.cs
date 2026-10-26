using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;
using WebApolice.SharedKernel.Application.Security;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.CriarModulo;

public class CriarModuloHandler : IRequestHandler<CriarModuloCommand, ModuloDto>
{
    private readonly IModulosCadastroRepository _dbContext;
    public CriarModuloHandler(IModulosCadastroRepository dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ModuloDto> Handle(CriarModuloCommand request, CancellationToken cancellationToken)
    {
        // Regra de negócio: Nome do módulo é único
        var existe = await _dbContext.NomeExisteAsync(request.Nome, null, cancellationToken);
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

        _dbContext.Adicionar(modulo);
        await _dbContext.SalvarAlteracoesAsync(cancellationToken);

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
