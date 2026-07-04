using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Domain;
using WebApolice.Modulos.Clientes.Domain.Exceptions;

namespace WebApolice.Modulos.Clientes.Application.UseCases.InativarCliente;

public sealed class InativarClienteHandler
{
    private readonly IClientesRepository _repository;
    private readonly IRegistradorAuditoria _auditoria;
    private readonly IClientesTransactionManager _transactionManager;

    public InativarClienteHandler(
        IClientesRepository repository, 
        IRegistradorAuditoria auditoria,
        IClientesTransactionManager transactionManager)
    {
        _repository = repository;
        _auditoria = auditoria;
        _transactionManager = transactionManager;
    }

    public async Task Handle(InativarClienteCommand command, string usuarioSub, CancellationToken cancellationToken)
    {
        var cliente = await _repository.ObterPorIdAsync(command.Id, cancellationToken);
        if (cliente == null)
            throw new ClienteNaoEncontradoException("Cliente não encontrado.");

        if (cliente.Status == StatusCliente.Inativo)
            return;

        cliente.Inativar();

        await _transactionManager.ExecuteInTransactionAsync(async () =>
        {
            await _repository.AtualizarAsync(cliente, cancellationToken);
            await _repository.SalvarAlteracoesAsync(cancellationToken);

            var auditoria = new RegistroAuditoria
            {
                UsuarioIdExterno = usuarioSub,
                Modulo = "clientes",
                Recurso = "cliente",
                RecursoId = cliente.Id.ToString(),
                Acao = "inativar",
                Resultado = ResultadoAuditoria.Sucesso,
                DadosAnteriores = JsonSerializer.SerializeToDocument(new { Status = StatusCliente.Ativo.ToString() }),
                DadosPosteriores = JsonSerializer.SerializeToDocument(new { Status = StatusCliente.Inativo.ToString() })
            };

            await _auditoria.RegistrarAsync(auditoria, cancellationToken);
        }, cancellationToken);
    }
}
