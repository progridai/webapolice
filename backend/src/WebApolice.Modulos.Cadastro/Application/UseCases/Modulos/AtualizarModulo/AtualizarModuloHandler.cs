using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.SharedKernel.Application.Exceptions;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.AtualizarModulo;

public class AtualizarModuloHandler : IRequestHandler<AtualizarModuloCommand, ModuloDto>
{
    private readonly IModulosCadastroRepository _dbContext;

    public AtualizarModuloHandler(IModulosCadastroRepository dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ModuloDto> Handle(AtualizarModuloCommand request, CancellationToken cancellationToken)
    {
        var modulo = await _dbContext.ObterAsync(request.PublicId, cancellationToken);
        if (modulo == null)
        {
            throw new ModuloNaoEncontradoException("Módulo não encontrado no catálogo.");
        }

        var existeOutroComNome = await _dbContext.NomeExisteAsync(request.Nome, modulo.Id, cancellationToken);
        if (existeOutroComNome)
        {
            throw new ValidacaoException("Já existe outro Módulo com este nome no catálogo.");
        }

        modulo.Nome = request.Nome;
        modulo.Descricao = request.Descricao;
        modulo.Ativo = request.Ativo;
        modulo.UpdatedAt = DateTimeOffset.UtcNow;

        _dbContext.Atualizar(modulo);
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
