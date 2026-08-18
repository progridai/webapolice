using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Models;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.ListarModulos;

public class ListarModulosHandler : IRequestHandler<ListarModulosQuery, PagedResult<ModuloListDto>>
{
    private readonly CadastroDbContext _dbContext;

    public ListarModulosHandler(CadastroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ModuloListDto>> Handle(ListarModulosQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Modulos.Where(m => m.DeletedAt == null).AsNoTracking();

        if (request.Ativo.HasValue)
        {
            query = query.Where(m => m.Ativo == request.Ativo.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var termo = $"%{request.Busca.ToLower()}%";
            query = query.Where(m => EF.Functions.ILike(m.Nome, termo));
        }

        var total = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderBy(m => m.Nome)
            .Skip((request.Pagina - 1) * request.TamanhoPagina)
            .Take(request.TamanhoPagina)
            .Select(m => new ModuloListDto(m.PublicId, m.Nome, m.Descricao, m.Ativo))
            .ToListAsync(cancellationToken);

        return new PagedResult<ModuloListDto>
        {
            Items = itens,
            TotalCount = total,
            Page = request.Pagina,
            PageSize = request.TamanhoPagina
        };
    }
}
