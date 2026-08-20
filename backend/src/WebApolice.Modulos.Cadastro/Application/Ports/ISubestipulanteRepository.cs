using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;

namespace WebApolice.Modulos.Cadastro.Application.Ports;

public interface ISubestipulanteRepository
{
    Task<SubestipulanteModel?> LocalizarPorIdAsync(long id, CancellationToken cancellationToken);
    Task<SubestipulanteModel?> ObterPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
    Task<bool> CnpjJaExisteAsync(string cnpjLimpo, long? desconsiderarSubestipulanteId, CancellationToken cancellationToken);

    void Adicionar(SubestipulanteModel model);
    void Atualizar(SubestipulanteModel model);

    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
