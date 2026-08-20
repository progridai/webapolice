using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AlterarApolice;

public sealed class AlterarApoliceHandler
{
    private readonly IApoliceRepository _repository;
    private readonly IRegistradorAuditoria _auditoria;

    public AlterarApoliceHandler(IApoliceRepository repository, IRegistradorAuditoria auditoria)
    {
        _repository = repository;
        _auditoria = auditoria;
    }

    public async Task Handle(AlterarApoliceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Nome))
            throw new InvalidOperationException("O Nome (denominação da apólice) é obrigatório.");

        if (command.DataFimVigencia.HasValue && command.DataFimVigencia < command.DataInicioVigencia)
            throw new InvalidOperationException("A data de fim de vigência não pode ser menor que a data de início.");

        await using var transaction = await _repository.BeginTransactionAsync(cancellationToken);

        try
        {
            var apolice = await _repository.ObterPorPublicIdAsync(command.PublicId, cancellationToken);
            
            if (apolice == null)
                throw new InvalidOperationException("Apólice não encontrada.");

            apolice.Nome = command.Nome;
            apolice.EstipulanteId = command.EstipulanteId;
            apolice.SeguradoraId = command.SeguradoraId;
            apolice.CorretoraId = command.CorretoraId;
            apolice.DataInicioVigencia = command.DataInicioVigencia;
            apolice.DataFimVigencia = command.DataFimVigencia;
            apolice.DataAniversario = command.DataAniversario;
            apolice.Observacao = command.Observacao;
            apolice.UpdatedAt = DateTimeOffset.UtcNow;
            
            // Incrementar versão se desejar (opcional, mas boa prática)
            apolice.Versao += 1;

            _repository.Atualizar(apolice);
            await _repository.SalvarAlteracoesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Auditoria
            await _auditoria.RegistrarAsync(new RegistroAuditoria
            {
                Acao = "APOLICE_ALTERADA",
                Modulo = "Seguros",
                Recurso = "apolice",
                RecursoId = apolice.PublicId.ToString(),
                Resultado = ResultadoAuditoria.Sucesso,
                DadosPosteriores = JsonSerializer.SerializeToDocument(new
                {
                    Nome = apolice.Nome,
                    EstipulanteId = apolice.EstipulanteId,
                    SeguradoraId = apolice.SeguradoraId
                })
            }, cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
