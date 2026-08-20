using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.InativarCorretora;

public sealed class InativarCorretoraHandler
{
    private readonly ICorretoraRepository _repository;

    public InativarCorretoraHandler(ICorretoraRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(InativarCorretoraCommand command, CancellationToken cancellationToken)
    {
        var corretora = await _repository.ObterPorPublicIdAsync(command.PublicId, cancellationToken);
        if (corretora == null)
            throw new InvalidOperationException("Corretora não encontrada.");

        if (!corretora.Ativo)
            return; // Já inativo

        corretora.Ativo = false;
        corretora.UpdatedAt = DateTimeOffset.UtcNow;

        _repository.Atualizar(corretora);
        await _repository.SalvarAlteracoesAsync(cancellationToken);
    }
}
