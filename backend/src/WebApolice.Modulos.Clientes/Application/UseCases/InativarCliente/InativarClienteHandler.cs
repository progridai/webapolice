using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Domain.Exceptions;

using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;

namespace WebApolice.Modulos.Clientes.Application.UseCases.InativarCliente;

public sealed record InativarClienteResult(Guid Id, string Status, DateTime UpdatedAt);

public sealed class InativarClienteHandler
{
    private readonly IClienteRepository _repository;
    private readonly IRegistradorAuditoria _auditoria;

    public InativarClienteHandler(IClienteRepository repository, IRegistradorAuditoria auditoria)
    {
        _repository = repository;
        _auditoria = auditoria;
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

        await _auditoria.RegistrarAsync(new RegistroAuditoria
        {
            Acao = "CLIENTE_INATIVADO",
            Modulo = "Clientes",
            Recurso = "cliente",
            RecursoId = cliente.PublicId.ToString(),
            Resultado = ResultadoAuditoria.Sucesso
        }, cancellationToken);

        return new InativarClienteResult(cliente.PublicId, statusInativo.Nome, cliente.UpdatedAt);
    }
}
