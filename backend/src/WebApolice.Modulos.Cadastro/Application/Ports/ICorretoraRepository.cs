using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;

namespace WebApolice.Modulos.Cadastro.Application.Ports;

public interface ICorretoraRepository
{
    void Adicionar(CorretoraModel corretora);
    void Atualizar(CorretoraModel corretora);
    Task<CorretoraModel?> ObterPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
    Task<bool> CorretoraExistePorPessoaIdAsync(long pessoaId, Guid? excetoPublicId, CancellationToken cancellationToken);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
