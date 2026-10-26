using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Auditoria.Contracts;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Modulos;

using WebApolice.Modulos.Seguranca.Application.Ports;

public record AlterarHabilitacaoModuloCommand(Guid PublicId, bool Habilitado);

public class AlterarHabilitacaoModuloUseCase
{
    private readonly SegurancaDbContext _dbContext;
    private readonly IOperacaoSegurancaAuditada _operacao;
    private readonly IContextoUsuarioAutenticado _contexto;

    public AlterarHabilitacaoModuloUseCase(SegurancaDbContext dbContext, IOperacaoSegurancaAuditada operacao, IContextoUsuarioAutenticado contexto)
    {
        _dbContext = dbContext;
        _operacao = operacao;
        _contexto = contexto;
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

        var evento = command.Habilitado ? "MODULO_HABILITADO" : "MODULO_DESABILITADO";
        await _operacao.ExecutarAsync<bool>("modulo", evento, async ct =>
        {
        if (command.Habilitado) modulo.Habilitar();
        else modulo.Desabilitar();


        var executor = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.KeycloakSub == _contexto.KeycloakSub, cancellationToken);
        var auditoriaInterna = new WebApolice.Modulos.Seguranca.Domain.Auditoria.AuditoriaPermissao(
            acao: evento,
            entidadeTipo: "MODULO",
            entidadeId: modulo.Id,
            usuarioExecutorId: executor?.Id,
            dadosNovos: JsonSerializer.SerializeToDocument(new { modulo.Codigo, modulo.Nome, modulo.Habilitado })
        );
        _dbContext.AuditoriaPermissoes.Add(auditoriaInterna);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return (true, modulo.PublicId.ToString(), null, JsonSerializer.SerializeToDocument(new { modulo.Codigo, modulo.Nome, modulo.Habilitado }));
        }, cancellationToken);
    }
}
