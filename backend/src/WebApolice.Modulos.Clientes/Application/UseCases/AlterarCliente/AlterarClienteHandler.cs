using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Domain.Exceptions;

namespace WebApolice.Modulos.Clientes.Application.UseCases.AlterarCliente;

public sealed class AlterarClienteHandler
{
    private readonly IClientesRepository _repository;
    private readonly IRegistradorAuditoria _auditoria;
    private readonly IClientesTransactionManager _transactionManager;

    public AlterarClienteHandler(
        IClientesRepository repository, 
        IRegistradorAuditoria auditoria,
        IClientesTransactionManager transactionManager)
    {
        _repository = repository;
        _auditoria = auditoria;
        _transactionManager = transactionManager;
    }

    public async Task Handle(AlterarClienteCommand command, string usuarioSub, CancellationToken cancellationToken)
    {
        var cliente = await _repository.ObterPorIdAsync(command.Id, cancellationToken);
        if (cliente == null)
            throw new ClienteNaoEncontradoException("Cliente não encontrado.");

        var dadosAnteriores = new
        {
            Nome = cliente.Nome,
            DataNascimento = cliente.DataNascimento?.ToString("O"),
            Email = cliente.Email,
            Telefone = cliente.Telefone
        };

        cliente.Alterar(
            command.Nome,
            command.DataNascimento,
            command.Email,
            command.Telefone);

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
                Acao = "alterar",
                Resultado = ResultadoAuditoria.Sucesso,
                DadosAnteriores = JsonSerializer.SerializeToDocument(dadosAnteriores),
                DadosPosteriores = JsonSerializer.SerializeToDocument(new
                {
                    Nome = cliente.Nome,
                    DataNascimento = cliente.DataNascimento?.ToString("O"),
                    Email = cliente.Email,
                    Telefone = cliente.Telefone
                })
            };

            await _auditoria.RegistrarAsync(auditoria, cancellationToken);
        }, cancellationToken);
    }
}
