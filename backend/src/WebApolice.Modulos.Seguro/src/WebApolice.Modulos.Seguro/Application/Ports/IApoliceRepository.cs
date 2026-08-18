using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Seguro.Application.Ports;

public interface IApoliceRepository
{
    Task<ApoliceModel?> ObterPorIdAsync(long apoliceId, CancellationToken cancellationToken);
    Task<ApoliceModel?> ObterPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
    
    Task<bool> ExisteApoliceParaSeguradoraAsync(long seguradoraId, CancellationToken cancellationToken);
    Task<bool> ExisteApoliceParaEstipulanteAsync(long estipulanteId, CancellationToken cancellationToken);

    void Adicionar(ApoliceModel apolice);
    void Atualizar(ApoliceModel apolice);

    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
    Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}
