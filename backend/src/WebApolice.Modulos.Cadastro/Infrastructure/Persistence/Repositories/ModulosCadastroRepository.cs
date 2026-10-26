using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.ListarModulos;
using WebApolice.Modulos.Cadastro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Models;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Repositories;

public sealed class ModulosCadastroRepository(CadastroDbContext _dbContext) : IModulosCadastroRepository
{
    public Task<ModuloModel?> ObterAsync(Guid publicId, CancellationToken ct) => _dbContext.Modulos.FirstOrDefaultAsync(m => m.PublicId == publicId && m.DeletedAt == null, ct);
    public Task<bool> NomeExisteAsync(string nome, long? desconsiderarId, CancellationToken ct) => _dbContext.Modulos.AnyAsync(m => m.Id != desconsiderarId && m.Nome.ToLower() == nome.ToLower() && m.DeletedAt == null, ct);
    public void Adicionar(ModuloModel modulo) => _dbContext.Modulos.Add(modulo);
    public void Atualizar(ModuloModel modulo) => _dbContext.Modulos.Update(modulo);
    public async Task SalvarAlteracoesAsync(CancellationToken ct) => await _dbContext.SaveChangesAsync(ct);
    public async Task<PagedResult<ModuloListDto>> ListarAsync(ListarModulosQuery request, CancellationToken cancellationToken)
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
            .Select(m => new ModuloListDto(m.PublicId, m.Nome, m.Descricao, m.Ativo, m.CreatedAt))
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
