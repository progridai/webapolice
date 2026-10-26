using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Domain;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Perfis;

public class CriarPerfilUseCase
{
    private readonly ISegurancaAdministracaoRepository _repository;
    private readonly IContextoUsuarioAutenticado _contexto;

    public CriarPerfilUseCase(ISegurancaAdministracaoRepository dbContext, IContextoUsuarioAutenticado contexto)
    {
        _repository = dbContext;
        _contexto = contexto;
    }

    public async Task<Guid> ExecuteAsync(
        string codigo,
        string nome,
        string descricao,
        bool ativo,
        List<Guid> permissaoPublicIds,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Código é obrigatório");
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Nome é obrigatório");

        var duplicado = await _repository.CodigoPerfilExisteAsync(codigo, cancellationToken);
        if (duplicado) throw new InvalidOperationException("Já existe um perfil com esse código.");

        var permissoesAtribuir = await _repository.ObterPermissoesAsync(permissaoPublicIds, true, cancellationToken);

        if (permissoesAtribuir.Count != permissaoPublicIds.Count)
        {
            throw new InvalidOperationException("Uma ou mais permissões são inválidas ou estão inativas.");
        }

        using var transaction = await _repository.BeginTransactionAsync(cancellationToken);

        var executor = await _repository.ObterUsuarioPorSubAsync(_contexto.KeycloakSub, cancellationToken);
        var executorId = executor?.Id;

        var novoPerfil = new Perfil(codigo, nome, descricao, ativo, false, false);

        _repository.AdicionarPerfil(novoPerfil);
        await _repository.SalvarAlteracoesAsync(cancellationToken);

        foreach (var permissao in permissoesAtribuir)
        {
            novoPerfil.Permissoes.Add(new WebApolice.Modulos.Seguranca.Domain.Relacionamentos.PerfilPermissao(
                perfilId: novoPerfil.Id,
                permissaoId: permissao.Id,
                atribuidoPorUsuarioId: executorId
            ));
        }

        var auditoria = new WebApolice.Modulos.Seguranca.Domain.Auditoria.AuditoriaPermissao(
            acao: "PERFIL_CRIADO",
            entidadeTipo: "PERFIL",
            entidadeId: novoPerfil.Id,
            perfilId: novoPerfil.Id,
            usuarioExecutorId: executorId,
            dadosNovos: JsonSerializer.SerializeToDocument(new { codigo, nome, descricao, ativo, permissoes = permissoesAtribuir.Select(p => p.Codigo) })
        );
        _repository.AdicionarAuditoria(auditoria);

        await _repository.SalvarAlteracoesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return novoPerfil.PublicId;
    }
}
