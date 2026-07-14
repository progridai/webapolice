using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Domain.Exceptions;

namespace WebApolice.Modulos.Clientes.Application.UseCases.InativarCliente;

public sealed record InativarClienteResult(Guid Id, string Status, DateTime UpdatedAt);

public sealed class InativarClienteHandler
{
    private readonly IClienteRepository _repository;

    public InativarClienteHandler(IClienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<InativarClienteResult> Handle(InativarClienteCommand command, CancellationToken cancellationToken)
    {
        var cliente = await _repository.ObterParaEdicaoPorPublicIdAsync(command.Id, cancellationToken);
        if (cliente == null)
            throw new ClienteNaoEncontradoException("Cliente não encontrado ou excluído.");

        var statusInativo = await _repository.ObterStatusPorCodigoAsync(WebApolice.Modulos.Clientes.Domain.ClienteStatusCodigos.Inativo, cancellationToken)
            ?? throw new ClienteInvalidoException($"Status '{WebApolice.Modulos.Clientes.Domain.ClienteStatusCodigos.Inativo}' não encontrado no catálogo.");

        cliente.Inativar(statusInativo.Id);

        await _repository.SalvarAlteracoesAsync(cancellationToken);

        return new InativarClienteResult(cliente.PublicId, statusInativo.Nome, cliente.UpdatedAt);
    }
}
