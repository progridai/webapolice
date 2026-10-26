using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ExcluirEstipulante;

public record ExcluirEstipulanteCommand(Guid PublicId);

public sealed class ExcluirEstipulanteHandler
{
    private readonly IEstipulanteRepository _repository;
    private readonly IOperacaoCadastroAuditada _operacao;

    public ExcluirEstipulanteHandler(IEstipulanteRepository repository, IOperacaoCadastroAuditada operacao)
    {
        _repository = repository;
        _operacao = operacao;
    }

    public async Task Handle(ExcluirEstipulanteCommand command, CancellationToken cancellationToken)
    {
        await _operacao.ExecutarAsync<bool>("Estipulantes", "estipulante", "ESTIPULANTE_EXCLUIDO", async ct =>
        {
            var estipulante = await _repository.ObterParaEdicaoPorPublicIdAsync(command.PublicId, cancellationToken);
            if (estipulante == null || estipulante.DeletedAt != null)
                throw new EstipulanteInvalidoException("Estipulante não encontrado ou já excluído.");

            estipulante.DeletedAt = DateTimeOffset.UtcNow;
            estipulante.Ativo = false;
            estipulante.UpdatedAt = DateTimeOffset.UtcNow;

            await _repository.SalvarAlteracoesAsync(cancellationToken);


            return (true, estipulante.PublicId.ToString(), null, JsonSerializer.SerializeToDocument(new { DeletedAt = estipulante.DeletedAt, Ativo = false }));
        }, cancellationToken);
    }
}
