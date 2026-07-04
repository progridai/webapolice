using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Domain;
using WebApolice.Modulos.Clientes.Domain.Exceptions;

namespace WebApolice.Modulos.Clientes.Application.UseCases.CadastrarCliente;

public sealed class CadastrarClienteHandler
{
    private readonly IClientesRepository _repository;
    private readonly IRegistradorAuditoria _auditoria;
    private readonly IClientesTransactionManager _transactionManager;

    public CadastrarClienteHandler(
        IClientesRepository repository, 
        IRegistradorAuditoria auditoria,
        IClientesTransactionManager transactionManager)
    {
        _repository = repository;
        _auditoria = auditoria;
        _transactionManager = transactionManager;
    }

    public async Task<CadastrarClienteResult> Handle(CadastrarClienteCommand command, string usuarioSub, CancellationToken cancellationToken)
    {
        // Criação usando a entidade
        var cliente = new Cliente(
            command.Nome,
            command.Cpf,
            command.DataNascimento,
            command.Email,
            command.Telefone,
            command.CodigoLegado);

        // Verificação prévia
        if (await _repository.ExisteCpfAsync(cliente.Cpf, cancellationToken))
            throw new ClienteJaCadastradoException("Já existe um cliente cadastrado com este CPF.");

        string? cpfMascarado = null;
        long clienteId = 0;
        DateTime dataCadastro = default;

        await _transactionManager.ExecuteInTransactionAsync(async () =>
        {
            await _repository.AdicionarAsync(cliente, cancellationToken);
            await _repository.SalvarAlteracoesAsync(cancellationToken);

            clienteId = cliente.Id;
            dataCadastro = cliente.DataCadastroUtc;
            cpfMascarado = "***.***.***-" + cliente.Cpf.Substring(cliente.Cpf.Length - 2);

            var auditoria = new RegistroAuditoria
            {
                UsuarioIdExterno = usuarioSub,
                Modulo = "clientes",
                Recurso = "cliente",
                RecursoId = cliente.Id.ToString(),
                Acao = "cadastrar",
                Resultado = ResultadoAuditoria.Sucesso,
                DadosPosteriores = JsonSerializer.SerializeToDocument(new
                {
                    Nome = cliente.Nome,
                    Cpf = cpfMascarado,
                    DataNascimento = cliente.DataNascimento?.ToString("O"),
                    Email = cliente.Email,
                    Telefone = cliente.Telefone,
                    CodigoLegado = cliente.CodigoLegado
                })
            };

            await _auditoria.RegistrarAsync(auditoria, cancellationToken);
        }, cancellationToken);

        return new CadastrarClienteResult(
            clienteId,
            cliente.Nome,
            cpfMascarado!,
            cliente.Status.ToString().ToLowerInvariant(),
            dataCadastro
        );
    }
}
