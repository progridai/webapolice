using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ReativarSubestipulante;

public sealed class ReativarSubestipulanteHandler
{
    private readonly ISubestipulanteRepository _repository;

    public ReativarSubestipulanteHandler(ISubestipulanteRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(ReativarSubestipulanteCommand command, CancellationToken cancellationToken)
    {
        var subestipulante = await _repository.ObterPorPublicIdAsync(command.PublicId, cancellationToken);
        
        if (subestipulante == null)
            throw new InvalidOperationException("Subestipulante não encontrado.");

        if (subestipulante.Ativo)
            return;

        subestipulante.Ativo = true;
        subestipulante.UpdatedAt = DateTimeOffset.UtcNow;

        _repository.Atualizar(subestipulante);
        await _repository.SalvarAlteracoesAsync(cancellationToken);
    }
}
