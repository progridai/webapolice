using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.InativarSubestipulante;

public sealed class InativarSubestipulanteHandler
{
    private readonly ISubestipulanteRepository _repository;

    public InativarSubestipulanteHandler(ISubestipulanteRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(InativarSubestipulanteCommand command, CancellationToken cancellationToken)
    {
        var subestipulante = await _repository.ObterPorPublicIdAsync(command.PublicId, cancellationToken);
        
        if (subestipulante == null)
            throw new InvalidOperationException("Subestipulante não encontrado.");

        if (!subestipulante.Ativo)
            return;

        subestipulante.Ativo = false;
        subestipulante.UpdatedAt = DateTimeOffset.UtcNow;

        _repository.Atualizar(subestipulante);
        await _repository.SalvarAlteracoesAsync(cancellationToken);
    }
}
