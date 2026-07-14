using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Domain.Exceptions;

namespace WebApolice.Modulos.Clientes.Application.UseCases.AtivarCliente;

public sealed record AtivarClienteResult(Guid Id, string Status, DateTime UpdatedAt);

public sealed class AtivarClienteHandler
{
    private readonly IClienteRepository _repository;

    public AtivarClienteHandler(IClienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<AtivarClienteResult> Handle(AtivarClienteCommand command, CancellationToken cancellationToken)
    {
        var cliente = await _repository.ObterParaEdicaoPorPublicIdAsync(command.Id, cancellationToken);
        if (cliente == null)
            throw new ClienteNaoEncontradoException("Cliente não encontrado ou excluído.");

        var statusAtivo = await _repository.ObterStatusPorCodigoAsync(WebApolice.Modulos.Clientes.Domain.ClienteStatusCodigos.Ativo, cancellationToken)
            ?? throw new ClienteInvalidoException($"Status '{WebApolice.Modulos.Clientes.Domain.ClienteStatusCodigos.Ativo}' não encontrado no catálogo.");

        cliente.Ativar(statusAtivo.Id);

        await _repository.SalvarAlteracoesAsync(cancellationToken);

        return new AtivarClienteResult(cliente.PublicId, statusAtivo.Nome, cliente.UpdatedAt);
    }
}
