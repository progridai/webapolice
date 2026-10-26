using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.SharedKernel.Application.Persistence;
using WebApolice.Modulos.Cadastro.Domain.Modelos.Vinculos;

namespace WebApolice.Modulos.Cadastro.Application.Ports;

public interface ISeguradoraRepository
{
    Task<SeguradoraModel?> LocalizarPorIdAsync(long id, CancellationToken cancellationToken);
    Task<SeguradoraModel?> ObterPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
    Task<bool> CnpjJaExisteAsync(string cnpjLimpo, long? desconsiderarSeguradoraId, CancellationToken cancellationToken);
    
    void Adicionar(SeguradoraModel seguradora);
    void Atualizar(SeguradoraModel seguradora);
    
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
    Task<ITransacaoPersistencia> BeginTransactionAsync(CancellationToken cancellationToken);
}
