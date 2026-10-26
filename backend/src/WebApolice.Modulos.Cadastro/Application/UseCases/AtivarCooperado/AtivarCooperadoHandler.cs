using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.AtivarCooperado;

public sealed record AtivarCooperadoCommand(Guid PublicId);

public sealed class AtivarCooperadoHandler
{
    private readonly ICooperadoRepository _repository;
    private readonly IOperacaoCadastroAuditada _operacao;

    public AtivarCooperadoHandler(ICooperadoRepository repository, IOperacaoCadastroAuditada operacao)
    {
        _repository = repository;
        _operacao = operacao;
    }

    public async Task Handle(AtivarCooperadoCommand command, string usuarioSub, CancellationToken cancellationToken)
    {
        await _operacao.ExecutarAsync<bool>("Cadastro", "COOPERADOS", "REATIVAR", async ct =>
        {
            var agenciador = await _repository.ObterPorPublicIdAsync(command.PublicId, cancellationToken)
                ?? throw new CooperadoNaoEncontradoException("Cooperado não encontrado.");

            agenciador.Ativar();

            await _repository.SalvarAlteracoesAsync(cancellationToken);

            var payloadAuditoria = JsonSerializer.SerializeToDocument(new { PublicId = agenciador.PublicId });

            return (true, agenciador.PublicId.ToString(), null, payloadAuditoria);
        }, cancellationToken);
    }
}
