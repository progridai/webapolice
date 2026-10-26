using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;

using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.InativarCliente;

public sealed record InativarClienteResult(Guid Id, string Status, DateTime UpdatedAt);

public sealed class InativarClienteHandler
{
    private readonly IClienteRepository _repository;
    private readonly IOperacaoCadastroAuditada _operacao;

    public InativarClienteHandler(IClienteRepository repository, IOperacaoCadastroAuditada operacao)
    {
        _repository = repository;
        _operacao = operacao;
    }

    public async Task<InativarClienteResult> Handle(InativarClienteCommand command, CancellationToken cancellationToken)
    {
        return await _operacao.ExecutarAsync<InativarClienteResult>("Clientes", "cliente", "CLIENTE_INATIVADO", async ct =>
        {
        var cliente = await _repository.ObterParaEdicaoPorPublicIdAsync(command.Id, cancellationToken);
        if (cliente == null)
            throw new ClienteNaoEncontradoException("Cliente não encontrado ou excluído.");

        var statusInativo = await _repository.ObterStatusPorCodigoAsync(WebApolice.Modulos.Cadastro.Domain.ClienteStatusCodigos.Inativo, cancellationToken)
            ?? throw new ClienteInvalidoException($"Status '{WebApolice.Modulos.Cadastro.Domain.ClienteStatusCodigos.Inativo}' não encontrado no catálogo.");

        cliente.Inativar(statusInativo.Id);

        await _repository.SalvarAlteracoesAsync(cancellationToken);

        var resultado = new InativarClienteResult(cliente.PublicId, statusInativo.Nome, cliente.UpdatedAt);
        return (resultado, cliente.PublicId.ToString(), null, null);
        }, cancellationToken);
    }
}
