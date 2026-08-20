using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarSubestipulante;
using WebApolice.Modulos.Cadastro.Application.UseCases.ListarSubestipulantes;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;

public class SubestipulantesQueries : ISubestipulantesQueries
{
    private readonly CadastroDbContext _dbContext;

    public SubestipulantesQueries(CadastroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(IEnumerable<SubestipulanteListagemItemResult> itens, int totalItens, int totalPaginas)> ListarPaginadoAsync(
        int pagina, 
        int tamanhoPagina, 
        string? busca, 
        bool? ativo, 
        CancellationToken cancellationToken)
    {
        var query = from s in _dbContext.Set<SubestipulanteModel>().AsNoTracking()
                    join p in _dbContext.Pessoas.AsNoTracking() on s.PessoaId equals p.Id
                    where s.DeletedAt == null
                    select new { s, p };

        if (ativo.HasValue)
            query = query.Where(x => x.s.Ativo == ativo.Value);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var buscaLower = busca.ToLower();
            query = query.Where(x => 
                (x.p.NomeNormalizado != null && x.p.NomeNormalizado.Contains(buscaLower)) || 
                (x.p.DocumentoPrincipal != null && x.p.DocumentoPrincipal.Contains(busca)) ||
                (x.p.DocumentoPrincipalLimpo != null && x.p.DocumentoPrincipalLimpo.Contains(busca)) ||
                (x.s.Codigo != null && x.s.Codigo.ToLower().Contains(buscaLower))
            );
        }

        var totalItens = await query.CountAsync(cancellationToken);
        var totalPaginas = (int)Math.Ceiling(totalItens / (double)tamanhoPagina);

        var itens = await query
            .OrderBy(x => x.p.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(x => new SubestipulanteListagemItemResult
            {
                PublicId = x.s.PublicId,
                Nome = x.p.Nome,
                Codigo = x.s.Codigo,
                Cnpj = x.p.DocumentoPrincipal,
                Ativo = x.s.Ativo,
                CreatedAt = x.s.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return (itens, totalItens, totalPaginas);
    }

    public async Task<SubestipulanteDetalheResult?> ObterPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        return await (from s in _dbContext.Set<SubestipulanteModel>().AsNoTracking()
                      join p in _dbContext.Pessoas.AsNoTracking() on s.PessoaId equals p.Id
                      where s.PublicId == publicId && s.DeletedAt == null
                      select new SubestipulanteDetalheResult
                      {
                          PublicId = s.PublicId,
                          Nome = p.Nome,
                          Codigo = s.Codigo,
                          Cnpj = p.DocumentoPrincipal,
                          CnpjLimpo = p.DocumentoPrincipalLimpo,
                          Ativo = s.Ativo,
                          Observacao = s.Observacao,
                          CreatedAt = s.CreatedAt,
                          UpdatedAt = s.UpdatedAt
                      }).FirstOrDefaultAsync(cancellationToken);
    }
}
