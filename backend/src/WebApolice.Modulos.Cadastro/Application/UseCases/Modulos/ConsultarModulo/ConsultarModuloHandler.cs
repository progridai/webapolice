using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.ConsultarModulo;

public class ConsultarModuloHandler : IRequestHandler<ConsultarModuloQuery, ModuloDto>
{
    private readonly CadastroDbContext _dbContext;

    public ConsultarModuloHandler(CadastroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ModuloDto> Handle(ConsultarModuloQuery request, CancellationToken cancellationToken)
    {
        var modulo = await _dbContext.Modulos
            .FirstOrDefaultAsync(m => m.PublicId == request.PublicId && m.DeletedAt == null, cancellationToken);

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
