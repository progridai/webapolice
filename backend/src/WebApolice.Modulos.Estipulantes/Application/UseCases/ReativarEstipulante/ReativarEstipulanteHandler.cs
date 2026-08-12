using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;
using WebApolice.Modulos.Estipulantes.Application.Ports;
using WebApolice.Modulos.Estipulantes.Domain.Exceptions;

namespace WebApolice.Modulos.Estipulantes.Application.UseCases.ReativarEstipulante;

public record ReativarEstipulanteCommand(Guid PublicId);

public sealed class ReativarEstipulanteHandler
{
    private readonly IEstipulanteRepository _repository;
    private readonly IRegistradorAuditoria _auditoria;

    public ReativarEstipulanteHandler(IEstipulanteRepository repository, IRegistradorAuditoria auditoria)
    {
        _repository = repository;
        _auditoria = auditoria;
    }

    public async Task Handle(ReativarEstipulanteCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await _repository.BeginTransactionAsync(cancellationToken);

        try
        {
            var estipulante = await _repository.ObterParaEdicaoPorPublicIdAsync(command.PublicId, cancellationToken);
            if (estipulante == null)
                throw new EstipulanteInvalidoException("Estipulante não encontrado ou excluído.");

            if (estipulante.Ativo)
            {
                // Idempotência
                return;
            }

            estipulante.Ativo = true;
            estipulante.UpdatedAt = DateTimeOffset.UtcNow;

            await _repository.SalvarAlteracoesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await _auditoria.RegistrarAsync(new RegistroAuditoria
            {
                Acao = "ESTIPULANTE_REATIVADO",
                Modulo = "Estipulantes",
                Recurso = "estipulante",
                RecursoId = estipulante.PublicId.ToString(),
                Resultado = ResultadoAuditoria.Sucesso,
                DadosPosteriores = JsonSerializer.SerializeToDocument(new { Ativo = true })
            }, cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
