using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Auditoria.Contracts;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;
using WebApolice.Modulos.Seguranca.Application.Ports;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Recursos;

public record AlterarHabilitacaoRecursoCommand(Guid PublicId, bool Habilitado);

public class AlterarHabilitacaoRecursoUseCase
{
    private readonly SegurancaDbContext _dbContext;
    private readonly IOperacaoSegurancaAuditada _operacao;
    private readonly IContextoUsuarioAutenticado _contexto;

    public AlterarHabilitacaoRecursoUseCase(SegurancaDbContext dbContext, IOperacaoSegurancaAuditada operacao, IContextoUsuarioAutenticado contexto)
    {
        _dbContext = dbContext;
        _operacao = operacao;
        _contexto = contexto;
    }

    public async Task ExecuteAsync(AlterarHabilitacaoRecursoCommand command, CancellationToken cancellationToken)
    {
        var recurso = await _dbContext.Recursos
            .Include(r => r.Modulo)
            .FirstOrDefaultAsync(r => r.PublicId == command.PublicId, cancellationToken);
            
        if (recurso == null)
            throw new InvalidOperationException($"Recurso {command.PublicId} não encontrado.");

        if (recurso.Habilitado == command.Habilitado)
            return;

        var evento = command.Habilitado ? "RECURSO_HABILITADO" : "RECURSO_DESABILITADO";
        await _operacao.ExecutarAsync<bool>("recurso", evento, async ct =>
        {
        if (command.Habilitado) recurso.Habilitar();
        else recurso.Desabilitar();


        var executor = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.KeycloakSub == _contexto.KeycloakSub, cancellationToken);
        var auditoriaInterna = new WebApolice.Modulos.Seguranca.Domain.Auditoria.AuditoriaPermissao(
            acao: evento,
            entidadeTipo: "RECURSO",
            entidadeId: recurso.Id,
            usuarioExecutorId: executor?.Id,
            dadosNovos: JsonSerializer.SerializeToDocument(new { recurso.Codigo, recurso.Nome, recurso.Habilitado, ModuloCodigo = recurso.Modulo.Codigo })
        );
        _dbContext.AuditoriaPermissoes.Add(auditoriaInterna);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return (true, recurso.PublicId.ToString(), null, JsonSerializer.SerializeToDocument(new { recurso.Codigo, recurso.Nome, recurso.Habilitado, ModuloCodigo = recurso.Modulo.Codigo }));
        }, cancellationToken);
    }
}
