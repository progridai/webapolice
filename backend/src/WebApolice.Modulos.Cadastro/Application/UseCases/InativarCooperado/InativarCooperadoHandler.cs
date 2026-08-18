using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.InativarCooperado;

public sealed record InativarCooperadoCommand(Guid PublicId, DateOnly DataDesligamento);

public sealed class InativarCooperadoHandler
{
    private readonly ICooperadoRepository _repository;
    private readonly CadastroDbContext _dbContext;
    private readonly IRegistradorAuditoria _auditoria;

    public InativarCooperadoHandler(ICooperadoRepository repository, CadastroDbContext dbContext, IRegistradorAuditoria auditoria)
    {
        _repository = repository;
        _dbContext = dbContext;
        _auditoria = auditoria;
    }

    public async Task Handle(InativarCooperadoCommand command, string usuarioSub, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var agenciador = await _repository.ObterPorPublicIdAsync(command.PublicId, cancellationToken)
                ?? throw new CooperadoNaoEncontradoException("Cooperado não encontrado.");

            agenciador.Inativar(command.DataDesligamento);

            await _repository.SalvarAlteracoesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var payloadAuditoria = JsonSerializer.SerializeToDocument(new { PublicId = agenciador.PublicId, DataDesligamento = command.DataDesligamento });
            await _auditoria.RegistrarAsync(new RegistroAuditoria { Acao = "INATIVAR", Modulo = "Cadastro", Recurso = "COOPERADOS", RecursoId = agenciador.PublicId.ToString(), Resultado = ResultadoAuditoria.Sucesso, DadosPosteriores = payloadAuditoria }, cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
