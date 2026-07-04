using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Domain;
using WebApolice.Modulos.Clientes.Domain.Exceptions;

namespace WebApolice.Modulos.Clientes.Infrastructure.Persistence;

public sealed class ClientesRepository : IClientesRepository
{
    private readonly ClientesDbContext _dbContext;

    public ClientesRepository(ClientesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken)
    {
        await _dbContext.Clientes.AddAsync(cliente, cancellationToken);
    }

    public Task AtualizarAsync(Cliente cliente, CancellationToken cancellationToken)
    {
        _dbContext.Clientes.Update(cliente);
        return Task.CompletedTask;
    }

    public async Task<bool> ExisteCpfAsync(string cpf, CancellationToken cancellationToken)
    {
        return await _dbContext.Clientes.AnyAsync(c => c.Cpf == cpf, cancellationToken);
    }

    public async Task<(IReadOnlyList<Cliente> Itens, int TotalItens, int TotalPaginas)> ListarPaginadoAsync(
        int pagina, 
        int tamanhoPagina, 
        string? nome, 
        string? cpf, 
        StatusCliente? status, 
        string? ordenarPor, 
        string? direcao, 
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Clientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(c => EF.Functions.ILike(c.Nome, $"%{nome}%"));

        if (!string.IsNullOrWhiteSpace(cpf))
            query = query.Where(c => c.Cpf == cpf);

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        var totalItens = await query.CountAsync(cancellationToken);
        var totalPaginas = (totalItens + tamanhoPagina - 1) / tamanhoPagina;

        // Ordenação segura
        bool descendente = !string.IsNullOrWhiteSpace(direcao) && direcao.ToLowerInvariant() == "desc";
        
        query = (ordenarPor?.ToLowerInvariant()) switch
        {
            "nome" => descendente ? query.OrderByDescending(c => c.Nome) : query.OrderBy(c => c.Nome),
            "data_cadastro" => descendente ? query.OrderByDescending(c => c.DataCadastroUtc) : query.OrderBy(c => c.DataCadastroUtc),
            _ => descendente ? query.OrderByDescending(c => c.Id) : query.OrderBy(c => c.Id)
        };

        var itens = await query
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);

        return (itens, totalItens, totalPaginas);
    }

    public async Task<Cliente?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken)
    {
        return await _dbContext.Clientes.FirstOrDefaultAsync(c => c.Cpf == cpf, cancellationToken);
    }

    public async Task<Cliente?> ObterPorIdAsync(long id, CancellationToken cancellationToken)
    {
        return await _dbContext.Clientes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("23505") == true)
        {
            // Tratamento genérico para restrição de chave única no Npgsql (23505 = unique_violation)
            throw new ClienteJaCadastradoException("Já existe um registro com os mesmos dados únicos (conflito).");
        }
    }
}
