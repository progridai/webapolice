using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.CriarApolice;

public sealed class CriarApoliceHandler
{
    private readonly IApoliceRepository _repository;
    private readonly IRegistradorAuditoria _auditoria;

    public CriarApoliceHandler(IApoliceRepository repository, IRegistradorAuditoria auditoria)
    {
        _repository = repository;
        _auditoria = auditoria;
    }

    public async Task<CriarApoliceResult> Handle(CriarApoliceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Nome))
            throw new InvalidOperationException("O Nome (denominação da apólice) é obrigatório.");

        if (command.DataFimVigencia.HasValue && command.DataFimVigencia < command.DataInicioVigencia)
            throw new InvalidOperationException("A data de fim de vigência não pode ser menor que a data de início.");

        await using var transaction = await _repository.BeginTransactionAsync(cancellationToken);

        try
        {
            var apolice = new ApoliceModel
            {
                PublicId = Guid.NewGuid(),
                EstipulanteId = command.EstipulanteId,
                SeguradoraId = command.SeguradoraId,
                CorretoraId = command.CorretoraId,
                Nome = command.Nome,
                DataInicioVigencia = command.DataInicioVigencia,
                DataFimVigencia = command.DataFimVigencia,
                DataAniversario = command.DataAniversario,
                Observacao = command.Observacao,
                Status = "ativa",
                Ativo = true,
                Versao = 1,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            // Processar Ramos
            if (command.Ramos != null && command.Ramos.Any())
            {
                foreach (var ramoCmd in command.Ramos)
                {
                    if (string.IsNullOrWhiteSpace(ramoCmd.TipoRamo)) continue;

                    apolice.Ramos.Add(new ApoliceRamoModel
                    {
                        TipoRamo = ramoCmd.TipoRamo.ToUpperInvariant(),
                        NumeroApolice = ramoCmd.NumeroApolice,
                        IofPercentual = ramoCmd.IofPercentual,
                        Ativo = true,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow
                    });
                }
            }

            // Processar Subestipulantes (vinculo inicial)
            if (command.SubestipulantesIds != null && command.SubestipulantesIds.Any())
            {
                foreach (var subId in command.SubestipulantesIds.Distinct())
                {
                    apolice.Subestipulantes.Add(new ApoliceSubestipulanteModel
                    {
                        SubestipulanteId = subId,
                        Ativo = true,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow
                    });
                }
            }

            _repository.Adicionar(apolice);
            await _repository.SalvarAlteracoesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Auditoria
            await _auditoria.RegistrarAsync(new RegistroAuditoria
            {
                Acao = "APOLICE_CRIADA",
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

            return new CriarApoliceResult(
                apolice.PublicId,
                apolice.Id,
                apolice.Nome,
                apolice.Status,
                apolice.CreatedAt
            );
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
