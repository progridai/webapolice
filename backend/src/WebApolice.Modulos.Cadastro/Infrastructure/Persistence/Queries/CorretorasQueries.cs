using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarCorretora;
using WebApolice.Modulos.Cadastro.Application.UseCases.ListarCorretoras;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;

public class CorretorasQueries : ICorretorasQueries
{
    private readonly CadastroDbContext _dbContext;

    public CorretorasQueries(CadastroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(IEnumerable<CorretoraListagemItemResult> itens, int totalItens, int totalPaginas)> ListarPaginadoAsync(
        int pagina, 
        int tamanhoPagina, 
        string? busca, 
        bool? ativo, 
        CancellationToken cancellationToken)
    {
        var query = from c in _dbContext.Corretoras.AsNoTracking()
                    join p in _dbContext.Pessoas.AsNoTracking() on c.PessoaId equals p.Id
                    where c.DeletedAt == null
                    select new { c, p };

        if (ativo.HasValue)
            query = query.Where(x => x.c.Ativo == ativo.Value);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var buscaLower = busca.ToLower();
            query = query.Where(x => 
                (x.p.NomeNormalizado != null && x.p.NomeNormalizado.Contains(buscaLower)) || 
                (x.p.DocumentoPrincipal != null && x.p.DocumentoPrincipal.Contains(busca)) ||
                (x.p.DocumentoPrincipalLimpo != null && x.p.DocumentoPrincipalLimpo.Contains(busca)) ||
                (x.c.Codigo != null && x.c.Codigo.ToLower().Contains(buscaLower)) ||
                (x.c.CodigoProtheus != null && x.c.CodigoProtheus.ToLower().Contains(buscaLower))
            );
        }

        var totalItens = await query.CountAsync(cancellationToken);
        var totalPaginas = (int)Math.Ceiling(totalItens / (double)tamanhoPagina);

        var itens = await query
            .OrderBy(x => x.p.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(x => new CorretoraListagemItemResult
            {
                PublicId = x.c.PublicId,
                Nome = x.p.Nome,
                Cnpj = x.p.DocumentoPrincipal,
                Codigo = x.c.Codigo,
                CodigoProtheus = x.c.CodigoProtheus,
                Ativo = x.c.Ativo,
                CreatedAt = x.c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return (itens, totalItens, totalPaginas);
    }

    public async Task<CorretoraDetalheResult?> ObterPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        return await (from c in _dbContext.Corretoras.AsNoTracking()
                      join p in _dbContext.Pessoas.AsNoTracking() on c.PessoaId equals p.Id
                      where c.PublicId == publicId && c.DeletedAt == null
                      select new CorretoraDetalheResult
                      {
                          PublicId = c.PublicId,
                          Nome = p.Nome,
                          Cnpj = p.DocumentoPrincipal,
                          CnpjLimpo = p.DocumentoPrincipalLimpo,
                          Codigo = c.Codigo,
                          CodigoProtheus = c.CodigoProtheus,
                          Ativo = c.Ativo,
                          Observacao = c.Observacao,
                          CreatedAt = c.CreatedAt,
                          UpdatedAt = c.UpdatedAt
                      }).FirstOrDefaultAsync(cancellationToken);
    }
}
