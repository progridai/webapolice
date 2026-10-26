using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.Modulos.Seguro.Application.DTOs;
using WebApolice.Modulos.Seguro.Application.Mappers;
using WebApolice.Modulos.Seguro.Application.UseCases.Ramos.ListarRamos;
using WebApolice.SharedKernel.Application.Models;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;

public sealed class RamosRepository(SeguroDbContext _context) : IRamosRepository
{
    public Task<RamoModel?> ObterAsync(Guid publicId, CancellationToken ct) => _context.Ramos.FirstOrDefaultAsync(r => r.PublicId == publicId, ct);
    public Task<RamoModel?> ObterParaLeituraAsync(Guid publicId, CancellationToken ct) => _context.Ramos.AsNoTracking().FirstOrDefaultAsync(r => r.PublicId == publicId, ct);
    public void Adicionar(RamoModel ramo) => _context.Ramos.Add(ramo);
    public void Atualizar(RamoModel ramo) => _context.Ramos.Update(ramo);
    public async Task SalvarAlteracoesAsync(CancellationToken ct) => await _context.SaveChangesAsync(ct);
    public async Task<PagedResult<RamoDto>> ListarAsync(ListarRamosQuery query, CancellationToken cancellationToken)
    {
        var dbQuery = _context.Ramos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Busca))
        {
            var busca = query.Busca.Trim().ToLower();
            dbQuery = dbQuery.Where(x =>
                EF.Functions.ILike(x.Nome, $"%{busca}%") ||
                EF.Functions.ILike(x.Codigo, $"%{busca}%"));
        }

        if (query.Ativo.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.Ativo == query.Ativo.Value);
        }

        var total = await dbQuery.CountAsync(cancellationToken);

        var items = await dbQuery
            .OrderBy(x => x.Nome)
            .Skip((query.Pagina - 1) * query.TamanhoPagina)
            .Take(query.TamanhoPagina)
            .Select(x => x.ToDto())
            .ToListAsync(cancellationToken);

        return new PagedResult<RamoDto>
        {
            Items = items,
            TotalCount = total,
            Page = query.Pagina,
            PageSize = query.TamanhoPagina
        };
    }
}
