using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.AtivarCooperado;

public sealed record AtivarCooperadoCommand(Guid PublicId);

public sealed class AtivarCooperadoHandler
{
    private readonly ICooperadoRepository _repository;
    private readonly CadastroDbContext _dbContext;
    private readonly IRegistradorAuditoria _auditoria;

    public AtivarCooperadoHandler(ICooperadoRepository repository, CadastroDbContext dbContext, IRegistradorAuditoria auditoria)
    {
        _repository = repository;
        _dbContext = dbContext;
        _auditoria = auditoria;
    }

    public async Task Handle(AtivarCooperadoCommand command, string usuarioSub, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var agenciador = await _repository.ObterPorPublicIdAsync(command.PublicId, cancellationToken)
                ?? throw new CooperadoNaoEncontradoException("Cooperado não encontrado.");

            agenciador.Ativar();

            await _repository.SalvarAlteracoesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var payloadAuditoria = JsonSerializer.SerializeToDocument(new { PublicId = agenciador.PublicId });
            await _auditoria.RegistrarAsync(new RegistroAuditoria { Acao = "REATIVAR", Modulo = "Cadastro", Recurso = "COOPERADOS", RecursoId = agenciador.PublicId.ToString(), Resultado = ResultadoAuditoria.Sucesso, DadosPosteriores = payloadAuditoria }, cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
