using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Repositories;

public class CorretoraRepository : ICorretoraRepository
{
    private readonly CadastroDbContext _dbContext;

    public CorretoraRepository(CadastroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Adicionar(CorretoraModel corretora)
    {
        _dbContext.Corretoras.Add(corretora);
    }

    public void Atualizar(CorretoraModel corretora)
    {
        _dbContext.Corretoras.Update(corretora);
    }

    public async Task<CorretoraModel?> ObterPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        return await _dbContext.Corretoras
            .Include(c => c.Pessoa)
            .FirstOrDefaultAsync(c => c.PublicId == publicId && c.DeletedAt == null, cancellationToken);
    }

    public async Task<bool> CorretoraExistePorPessoaIdAsync(long pessoaId, Guid? excetoPublicId, CancellationToken cancellationToken)
    {
        var query = _dbContext.Corretoras.Where(c => c.PessoaId == pessoaId && c.DeletedAt == null);
        
        if (excetoPublicId.HasValue)
        {
            query = query.Where(c => c.PublicId != excetoPublicId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
