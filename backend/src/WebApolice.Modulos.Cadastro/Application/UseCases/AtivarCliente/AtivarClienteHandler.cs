using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;

using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.AtivarCliente;

public sealed record AtivarClienteResult(Guid Id, string Status, DateTime UpdatedAt);

public sealed class AtivarClienteHandler
{
    private readonly IClienteRepository _repository;
    private readonly IOperacaoCadastroAuditada _operacao;

    public AtivarClienteHandler(IClienteRepository repository, IOperacaoCadastroAuditada operacao)
    {
        _repository = repository;
        _operacao = operacao;
    }

    public async Task<AtivarClienteResult> Handle(AtivarClienteCommand command, CancellationToken cancellationToken)
    {
        return await _operacao.ExecutarAsync<AtivarClienteResult>("Clientes", "cliente", "CLIENTE_ATIVADO", async ct =>
        {
        var cliente = await _repository.ObterParaEdicaoPorPublicIdAsync(command.Id, cancellationToken);
        if (cliente == null)
            throw new ClienteNaoEncontradoException("Cliente não encontrado ou excluído.");

        var statusAtivo = await _repository.ObterStatusPorCodigoAsync(WebApolice.Modulos.Cadastro.Domain.ClienteStatusCodigos.Ativo, cancellationToken)
            ?? throw new ClienteInvalidoException($"Status '{WebApolice.Modulos.Cadastro.Domain.ClienteStatusCodigos.Ativo}' não encontrado no catálogo.");

        cliente.Ativar(statusAtivo.Id);

        await _repository.SalvarAlteracoesAsync(cancellationToken);

        var resultado = new AtivarClienteResult(cliente.PublicId, statusAtivo.Nome, cliente.UpdatedAt);
        return (resultado, cliente.PublicId.ToString(), null, null);
        }, cancellationToken);
    }
}
