using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Repositories;

public class SeguradoraRepository : ISeguradoraRepository
{
    private readonly CadastroDbContext _dbContext;

    public SeguradoraRepository(CadastroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SeguradoraModel?> LocalizarPorIdAsync(long id, CancellationToken cancellationToken)
    {
        return await _dbContext.Seguradoras
            .Where(s => s.Id == id && s.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SeguradoraModel?> ObterPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        return await _dbContext.Seguradoras
            .Where(s => s.PublicId == publicId && s.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> CnpjJaExisteAsync(string cnpjLimpo, long? desconsiderarSeguradoraId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cnpjLimpo)) return false;

        return await (from s in _dbContext.Seguradoras
                      join p in _dbContext.Pessoas on s.PessoaId equals p.Id
                      where p.DocumentoPrincipalLimpo == cnpjLimpo && s.DeletedAt == null && (!desconsiderarSeguradoraId.HasValue || s.Id != desconsiderarSeguradoraId.Value)
                      select s.Id).AnyAsync(cancellationToken);
    }

    public void Adicionar(SeguradoraModel seguradora)
    {
        _dbContext.Seguradoras.Add(seguradora);
    }

    public void Atualizar(SeguradoraModel seguradora)
    {
        _dbContext.Seguradoras.Update(seguradora);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }
}
