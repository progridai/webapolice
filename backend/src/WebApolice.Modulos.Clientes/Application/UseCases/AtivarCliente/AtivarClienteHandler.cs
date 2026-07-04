using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Domain;
using WebApolice.Modulos.Clientes.Domain.Exceptions;

namespace WebApolice.Modulos.Clientes.Application.UseCases.AtivarCliente;

public sealed class AtivarClienteHandler
{
    private readonly IClientesRepository _repository;
    private readonly IRegistradorAuditoria _auditoria;
    private readonly IClientesTransactionManager _transactionManager;

    public AtivarClienteHandler(
        IClientesRepository repository, 
        IRegistradorAuditoria auditoria,
        IClientesTransactionManager transactionManager)
    {
        _repository = repository;
        _auditoria = auditoria;
        _transactionManager = transactionManager;
    }

    public async Task Handle(AtivarClienteCommand command, string usuarioSub, CancellationToken cancellationToken)
    {
        var cliente = await _repository.ObterPorIdAsync(command.Id, cancellationToken);
        if (cliente == null)
            throw new ClienteNaoEncontradoException("Cliente não encontrado.");

        if (cliente.Status == StatusCliente.Ativo)
            return;

        cliente.Ativar();

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
                Acao = "ativar",
                Resultado = ResultadoAuditoria.Sucesso,
                DadosAnteriores = JsonSerializer.SerializeToDocument(new { Status = StatusCliente.Inativo.ToString() }),
                DadosPosteriores = JsonSerializer.SerializeToDocument(new { Status = StatusCliente.Ativo.ToString() })
            };

            await _auditoria.RegistrarAsync(auditoria, cancellationToken);
        }, cancellationToken);
    }
}
