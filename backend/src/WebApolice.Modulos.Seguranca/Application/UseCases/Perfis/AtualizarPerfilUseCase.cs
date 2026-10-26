using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Domain;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Perfis;

public class AtualizarPerfilUseCase
{
    private readonly ISegurancaAdministracaoRepository _repository;
    private readonly IContextoUsuarioAutenticado _contexto;

    public AtualizarPerfilUseCase(ISegurancaAdministracaoRepository dbContext, IContextoUsuarioAutenticado contexto)
    {
        _repository = dbContext;
        _contexto = contexto;
    }

    public async Task ExecuteAsync(
        Guid publicId,
        string nome,
        string descricao,
        bool ativo,
        List<Guid> permissaoPublicIds,
        CancellationToken cancellationToken)
    {
        var perfil = await _repository.ObterPerfilComPermissoesAsync(publicId, cancellationToken);

        if (perfil == null) throw new InvalidOperationException("Perfil não encontrado.");
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Nome é obrigatório");

        if (perfil.PerfilSistema && perfil.Codigo == "ADMINISTRADOR")
        {
            throw new InvalidOperationException("Não é permitido alterar o perfil ADMINISTRADOR. Ele é um perfil de sistema com acesso total.");
        }

        var permissoesAtribuir = await _repository.ObterPermissoesAsync(permissaoPublicIds, false, cancellationToken);

        var executor = await _repository.ObterUsuarioPorSubAsync(_contexto.KeycloakSub, cancellationToken);
        var executorId = executor?.Id;

        using var transaction = await _repository.BeginTransactionAsync(cancellationToken);

        var dadosAnteriores = JsonSerializer.SerializeToDocument(new { nome = perfil.Nome, descricao = perfil.Descricao, ativo = perfil.Ativo, permissoes = perfil.Permissoes.Select(p => p.Permissao.Codigo) });

        perfil.Atualizar(nome, descricao, ativo);

        var permissoesRemover = perfil.Permissoes.Where(pp => !permissaoPublicIds.Contains(pp.Permissao.PublicId)).ToList();
        foreach (var pr in permissoesRemover)
        {
            perfil.Permissoes.Remove(pr);
        }

        var permissoesAdicionar = permissoesAtribuir.Where(p => !perfil.Permissoes.Any(pp => pp.PermissaoId == p.Id)).ToList();
        foreach (var pa in permissoesAdicionar)
        {
            perfil.Permissoes.Add(new WebApolice.Modulos.Seguranca.Domain.Relacionamentos.PerfilPermissao(
                perfilId: perfil.Id,
                permissaoId: pa.Id,
                atribuidoPorUsuarioId: executorId
            ));
        }

        await _repository.SalvarAlteracoesAsync(cancellationToken);

        var permissoesAtuais = perfil.Permissoes.Select(p => p.Permissao).ToList();

        var auditoria = new WebApolice.Modulos.Seguranca.Domain.Auditoria.AuditoriaPermissao(
            acao: "PERFIL_ALTERADO",
            entidadeTipo: "PERFIL",
            entidadeId: perfil.Id,
            perfilId: perfil.Id,
            usuarioExecutorId: executor?.Id,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: JsonSerializer.SerializeToDocument(new { perfil.Nome, perfil.Descricao, perfil.Ativo, Permissoes = permissoesAtuais.Select(p => p.Codigo) })
        );
        _repository.AdicionarAuditoria(auditoria);

        await _repository.SalvarAlteracoesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
