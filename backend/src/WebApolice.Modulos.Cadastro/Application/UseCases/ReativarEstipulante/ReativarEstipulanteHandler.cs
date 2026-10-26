using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ReativarEstipulante;

public record ReativarEstipulanteCommand(Guid PublicId);

public sealed class ReativarEstipulanteHandler
{
    private readonly IEstipulanteRepository _repository;
    private readonly IOperacaoCadastroAuditada _operacao;

    public ReativarEstipulanteHandler(IEstipulanteRepository repository, IOperacaoCadastroAuditada operacao)
    {
        _repository = repository;
        _operacao = operacao;
    }

    public async Task Handle(ReativarEstipulanteCommand command, CancellationToken cancellationToken)
    {
            var estipulante = await _repository.ObterParaEdicaoPorPublicIdAsync(command.PublicId, cancellationToken);
            if (estipulante == null)
                throw new EstipulanteInvalidoException("Estipulante não encontrado ou excluído.");

            if (estipulante.Ativo)
            {
                // Idempotência
                return;
            }

        await _operacao.ExecutarAsync<bool>("Estipulantes", "estipulante", "ESTIPULANTE_REATIVADO", async ct =>
        {


            estipulante.Ativo = true;
            estipulante.UpdatedAt = DateTimeOffset.UtcNow;

            await _repository.SalvarAlteracoesAsync(cancellationToken);


            return (true, estipulante.PublicId.ToString(), null, JsonSerializer.SerializeToDocument(new { Ativo = true }));
        }, cancellationToken);
    }
}
