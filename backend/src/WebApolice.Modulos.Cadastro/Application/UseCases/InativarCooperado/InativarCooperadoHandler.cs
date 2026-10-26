using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.InativarCooperado;

public sealed record InativarCooperadoCommand(Guid PublicId, DateOnly DataDesligamento);

public sealed class InativarCooperadoHandler
{
    private readonly ICooperadoRepository _repository;
    private readonly IOperacaoCadastroAuditada _operacao;

    public InativarCooperadoHandler(ICooperadoRepository repository, IOperacaoCadastroAuditada operacao)
    {
        _repository = repository;
        _operacao = operacao;
    }

    public async Task Handle(InativarCooperadoCommand command, string usuarioSub, CancellationToken cancellationToken)
    {
        await _operacao.ExecutarAsync<bool>("Cadastro", "COOPERADOS", "INATIVAR", async ct =>
        {
            var agenciador = await _repository.ObterPorPublicIdAsync(command.PublicId, cancellationToken)
                ?? throw new CooperadoNaoEncontradoException("Cooperado não encontrado.");

            agenciador.Inativar(command.DataDesligamento);

            await _repository.SalvarAlteracoesAsync(cancellationToken);

            var payloadAuditoria = JsonSerializer.SerializeToDocument(new { PublicId = agenciador.PublicId, DataDesligamento = command.DataDesligamento });

            return (true, agenciador.PublicId.ToString(), null, payloadAuditoria);
        }, cancellationToken);
    }
}
