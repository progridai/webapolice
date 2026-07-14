using System;
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

    public async Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Clientes.FirstOrDefaultAsync(c => c.PublicId == id, cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("23505") == true)
        {
            throw new ClienteJaCadastradoException("Já existe um registro com os mesmos dados únicos (conflito).");
        }
    }
}
