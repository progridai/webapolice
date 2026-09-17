using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarModulos;

public class ListarModulosApoliceHandler : IRequestHandler<ListarModulosApoliceQuery, IEnumerable<ModuloApoliceDto>>
{
    private readonly SeguroDbContext _dbContext;

    public ListarModulosApoliceHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<ModuloApoliceDto>> Handle(ListarModulosApoliceQuery request, CancellationToken cancellationToken)
    {
        var apolice = await _dbContext.Apolices
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.PublicId == request.ApolicePublicId && a.DeletedAt == null, cancellationToken);

        if (apolice == null)
            throw new ValidacaoException("Apólice não encontrada.");

        // Realizamos um join via LINQ com SQL interpolation para obter informações do módulo no cadastro global
        // Isso assume que o EF Core consegue compor. Senão, faremos em duas etapas.
        // Dado que estão em schemas diferentes no mesmo banco, o EF deve conseguir se fizermos via FromSql.
        // Mas a abordagem mais simples, já que a volumetria de módulos por apólice é minúscula (geralmente < 5):
        
        var vinculos = await _dbContext.ApoliceModulos
            .AsNoTracking()
            .Where(m => m.ApoliceId == apolice.Id && m.DeletedAt == null)
            .ToListAsync(cancellationToken);

        if (!vinculos.Any())
            return new List<ModuloApoliceDto>();

        var modulosIds = string.Join(",", vinculos.Select(v => v.ModuloId));

        var modulosGlobais = await _dbContext.Database
            .SqlQueryRaw<ModuloGlobalDto>($"SELECT id AS \"Id\", public_id AS \"PublicId\", nome AS \"Nome\", descricao AS \"Descricao\", ativo AS \"Ativo\" FROM cadastro.modulo WHERE id IN ({modulosIds})")
            .ToListAsync(cancellationToken);

        var result = vinculos.Select(v => 
        {
            var mg = modulosGlobais.First(m => m.Id == v.ModuloId);
            return new ModuloApoliceDto
            {
                PublicId = v.PublicId,
                ModuloPublicId = mg.PublicId,
                Nome = mg.Nome,
                Descricao = mg.Descricao,
                DataInicio = v.DataInicio,
                DataFim = v.DataFim,
                Observacao = v.Observacao,
                Ativo = v.Ativo
            };
        }).ToList();

        return result;
    }
}

internal class ModuloGlobalDto
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public string Nome { get; set; } = null!;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; }
}
