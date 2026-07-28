using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Auditoria.Contracts;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Modulos;

public record AlterarHabilitacaoModuloCommand(Guid PublicId, bool Habilitado);

public class AlterarHabilitacaoModuloUseCase
{
    private readonly SegurancaDbContext _dbContext;
    private readonly IRegistradorAuditoria _auditoria;

    public AlterarHabilitacaoModuloUseCase(SegurancaDbContext dbContext, IRegistradorAuditoria auditoria)
    {
        _dbContext = dbContext;
        _auditoria = auditoria;
    }

    public async Task ExecuteAsync(AlterarHabilitacaoModuloCommand command, CancellationToken cancellationToken)
    {
        var modulo = await _dbContext.Modulos.FirstOrDefaultAsync(m => m.PublicId == command.PublicId, cancellationToken);
        if (modulo == null)
            throw new InvalidOperationException($"Módulo {command.PublicId} não encontrado.");

        if (modulo.Codigo == "SEGURANCA" && !command.Habilitado)
            throw new InvalidOperationException("O módulo SEGURANCA não pode ser desabilitado.");

        if (modulo.Habilitado == command.Habilitado)
            return;

        if (command.Habilitado)
            modulo.Habilitar();
        else
            modulo.Desabilitar();

        var evento = command.Habilitado ? "MODULO_HABILITADO" : "MODULO_DESABILITADO";

        await _auditoria.RegistrarAsync(new WebApolice.Auditoria.Domain.RegistroAuditoria
        {
            Acao = evento,
            Modulo = "Seguranca",
            Recurso = "modulo",
            RecursoId = modulo.PublicId.ToString(),
            Resultado = WebApolice.Auditoria.Domain.ResultadoAuditoria.Sucesso,
            DadosPosteriores = JsonSerializer.SerializeToDocument(new { modulo.Codigo, modulo.Nome, modulo.Habilitado })
        }, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
