using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Auditoria.Contracts;
using WebApolice.Modulos.Seguranca.Application.Ports;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Modulos;


public record AlterarHabilitacaoModuloCommand(Guid PublicId, bool Habilitado);

public class AlterarHabilitacaoModuloUseCase
{
    private readonly ISegurancaAdministracaoRepository _repository;
    private readonly IOperacaoSegurancaAuditada _operacao;
    private readonly IContextoUsuarioAutenticado _contexto;

    public AlterarHabilitacaoModuloUseCase(ISegurancaAdministracaoRepository dbContext, IOperacaoSegurancaAuditada operacao, IContextoUsuarioAutenticado contexto)
    {
        _repository = dbContext;
        _operacao = operacao;
        _contexto = contexto;
    }

    public async Task ExecuteAsync(AlterarHabilitacaoModuloCommand command, CancellationToken cancellationToken)
    {
        var modulo = await _repository.ObterModuloAsync(command.PublicId, cancellationToken);
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


        var executor = await _repository.ObterUsuarioPorSubAsync(_contexto.KeycloakSub, cancellationToken);
        var auditoriaInterna = new WebApolice.Modulos.Seguranca.Domain.Auditoria.AuditoriaPermissao(
            acao: evento,
            entidadeTipo: "MODULO",
            entidadeId: modulo.Id,
            usuarioExecutorId: executor?.Id,
            dadosNovos: JsonSerializer.SerializeToDocument(new { modulo.Codigo, modulo.Nome, modulo.Habilitado })
        );
        _repository.AdicionarAuditoria(auditoriaInterna);

        await _repository.SalvarAlteracoesAsync(cancellationToken);
        return (true, modulo.PublicId.ToString(), null, JsonSerializer.SerializeToDocument(new { modulo.Codigo, modulo.Nome, modulo.Habilitado }));
        }, cancellationToken);
    }
}
