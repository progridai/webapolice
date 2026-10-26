using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;
using WebApolice.Modulos.Cadastro.Application.Ports;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.ConsultarModulo;

public class ConsultarModuloHandler : IRequestHandler<ConsultarModuloQuery, ModuloDto>
{
    private readonly IModulosCadastroRepository _dbContext;

    public ConsultarModuloHandler(IModulosCadastroRepository dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ModuloDto> Handle(ConsultarModuloQuery request, CancellationToken cancellationToken)
    {
        var modulo = await _dbContext.ObterAsync(request.PublicId, cancellationToken);

        if (modulo == null)
        {
            throw new ModuloNaoEncontradoException("Módulo não encontrado no catálogo.");
        }

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
