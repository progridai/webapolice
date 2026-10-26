using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.SharedKernel.Application.Models;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.ListarModulos;

public class ListarModulosHandler : IRequestHandler<ListarModulosQuery, PagedResult<ModuloListDto>>
{
    private readonly IModulosCadastroRepository _dbContext;

    public ListarModulosHandler(IModulosCadastroRepository dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PagedResult<ModuloListDto>> Handle(ListarModulosQuery request, CancellationToken cancellationToken)
        => _dbContext.ListarAsync(request, cancellationToken);
}
