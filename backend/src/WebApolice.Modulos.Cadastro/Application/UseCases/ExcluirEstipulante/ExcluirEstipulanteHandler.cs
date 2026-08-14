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
    private readonly IRegistradorAuditoria _auditoria;

    public ExcluirEstipulanteHandler(IEstipulanteRepository repository, IRegistradorAuditoria auditoria)
    {
        _repository = repository;
        _auditoria = auditoria;
    }

    public async Task Handle(ExcluirEstipulanteCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await _repository.BeginTransactionAsync(cancellationToken);

        try
        {
            var estipulante = await _repository.ObterParaEdicaoPorPublicIdAsync(command.PublicId, cancellationToken);
            if (estipulante == null || estipulante.DeletedAt != null)
                throw new EstipulanteInvalidoException("Estipulante nÃ£o encontrado ou jÃ¡ excluÃ­do.");

            estipulante.DeletedAt = DateTimeOffset.UtcNow;
            estipulante.Ativo = false;
            estipulante.UpdatedAt = DateTimeOffset.UtcNow;

            await _repository.SalvarAlteracoesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await _auditoria.RegistrarAsync(new RegistroAuditoria
            {
                Acao = "ESTIPULANTE_EXCLUIDO",
                Modulo = "Estipulantes",
                Recurso = "estipulante",
                RecursoId = estipulante.PublicId.ToString(),
                Resultado = ResultadoAuditoria.Sucesso,
                DadosPosteriores = JsonSerializer.SerializeToDocument(new { DeletedAt = estipulante.DeletedAt, Ativo = false })
            }, cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
