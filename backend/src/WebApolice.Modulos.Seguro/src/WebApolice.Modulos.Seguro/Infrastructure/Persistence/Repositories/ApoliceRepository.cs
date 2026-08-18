using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;

public class ApoliceRepository : IApoliceRepository
{
    private readonly SeguroDbContext _dbContext;

    public ApoliceRepository(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApoliceModel?> ObterPorIdAsync(long apoliceId, CancellationToken cancellationToken)
    {
        return await _dbContext.Apolices
            .Include(a => a.Ramos)
            .Include(a => a.Subestipulantes)
            .FirstOrDefaultAsync(a => a.Id == apoliceId && a.DeletedAt == null, cancellationToken);
    }

    public async Task<ApoliceModel?> ObterPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        return await _dbContext.Apolices
            .Include(a => a.Ramos)
            .Include(a => a.Subestipulantes)
            .FirstOrDefaultAsync(a => a.PublicId == publicId && a.DeletedAt == null, cancellationToken);
    }

    public async Task<bool> ExisteApoliceParaSeguradoraAsync(long seguradoraId, CancellationToken cancellationToken)
    {
        return await _dbContext.Apolices
            .AnyAsync(a => a.SeguradoraId == seguradoraId && a.DeletedAt == null, cancellationToken);
    }

    public async Task<bool> ExisteApoliceParaEstipulanteAsync(long estipulanteId, CancellationToken cancellationToken)
    {
        return await _dbContext.Apolices
            .AnyAsync(a => a.EstipulanteId == estipulanteId && a.DeletedAt == null, cancellationToken);
    }

    public void Adicionar(ApoliceModel apolice)
    {
        _dbContext.Apolices.Add(apolice);
    }

    public void Atualizar(ApoliceModel apolice)
    {
        _dbContext.Apolices.Update(apolice);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }
}
