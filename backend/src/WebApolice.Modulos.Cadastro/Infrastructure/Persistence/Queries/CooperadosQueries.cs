using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.SharedKernel.Application;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;

public sealed class CooperadosQueries : ICooperadosQueries
{
    private readonly CadastroDbContext _dbContext;

    public CooperadosQueries(CadastroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WebApolice.Modulos.Cadastro.Application.UseCases.ListarClientes.ListagemPaginadaResult<CooperadoListDto>> ListarAsync(int pagina, int tamanhoPagina, string? termoBusca, short? tipo, CancellationToken cancellationToken)
    {
        var query = from a in _dbContext.Agenciadores
                    join p in _dbContext.Pessoas on a.PessoaId equals p.Id
                    where a.DeletedAt == null
                    select new { a, p };

        if (tipo.HasValue)
        {
            query = query.Where(x => (short)x.a.Tipo == tipo.Value);
        }

        if (!string.IsNullOrWhiteSpace(termoBusca))
        {
            var termo = termoBusca.Trim().ToUpperInvariant();
            query = query.Where(x => 
                (x.p.NomeNormalizado != null && x.p.NomeNormalizado.Contains(termo)) ||
                (x.p.DocumentoPrincipalLimpo != null && x.p.DocumentoPrincipalLimpo.Contains(termo)) ||
                (x.a.Codigo != null && x.a.Codigo.Contains(termo))
            );
        }

        var total = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderByDescending(x => x.a.CreatedAt)
            .Skip((System.Math.Max(1, pagina) - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(x => new CooperadoListDto(
                x.a.PublicId,
                x.p.Nome,
                x.p.DocumentoPrincipalLimpo != null && x.p.DocumentoPrincipalLimpo.Length == 11 
                    ? $"{x.p.DocumentoPrincipalLimpo.Substring(0, 3)}.***.***-{x.p.DocumentoPrincipalLimpo.Substring(9, 2)}"
                    : "***",
                (short)x.a.Tipo,
                x.a.Codigo,
                x.a.Desativado,
                x.a.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var totalPaginas = (int)System.Math.Ceiling(total / (double)tamanhoPagina);
        return new WebApolice.Modulos.Cadastro.Application.UseCases.ListarClientes.ListagemPaginadaResult<CooperadoListDto>(itens, pagina, tamanhoPagina, total, totalPaginas);
    }
}
