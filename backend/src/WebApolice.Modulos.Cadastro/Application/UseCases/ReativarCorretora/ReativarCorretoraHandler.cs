using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ReativarCorretora;

public sealed class ReativarCorretoraHandler
{
    private readonly ICorretoraRepository _repository;

    public ReativarCorretoraHandler(ICorretoraRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(ReativarCorretoraCommand command, CancellationToken cancellationToken)
    {
        var corretora = await _repository.ObterPorPublicIdAsync(command.PublicId, cancellationToken);
        if (corretora == null)
            throw new InvalidOperationException("Corretora não encontrada.");

        if (corretora.Ativo)
            return; // Já ativo

        corretora.Ativo = true;
        corretora.UpdatedAt = DateTimeOffset.UtcNow;

        _repository.Atualizar(corretora);
        await _repository.SalvarAlteracoesAsync(cancellationToken);
    }
}
