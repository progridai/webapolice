using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Domain;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Perfis;

public class AtualizarPerfilUseCase
{
    private readonly SegurancaDbContext _dbContext;
    private readonly IContextoUsuarioAutenticado _contexto;

    public AtualizarPerfilUseCase(SegurancaDbContext dbContext, IContextoUsuarioAutenticado contexto)
    {
        _dbContext = dbContext;
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
        var perfil = await _dbContext.Perfis
            .Include(p => p.Permissoes)
            .ThenInclude(pp => pp.Permissao)
            .FirstOrDefaultAsync(p => p.PublicId == publicId, cancellationToken);

        if (perfil == null) throw new InvalidOperationException("Perfil não encontrado.");
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Nome é obrigatório");

        if (perfil.PerfilSistema && perfil.Codigo == "ADMINISTRADOR")
        {
            throw new InvalidOperationException("Não é permitido alterar o perfil ADMINISTRADOR. Ele é um perfil de sistema com acesso total.");
        }

        var permissoesAtribuir = await _dbContext.Permissoes
            .Where(p => permissaoPublicIds.Contains(p.PublicId))
            .ToListAsync(cancellationToken);

        var executor = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.KeycloakSub == _contexto.KeycloakSub, cancellationToken);
        var executorId = executor?.Id;

        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var dadosAnteriores = JsonSerializer.Serialize(new { nome = perfil.Nome, descricao = perfil.Descricao, ativo = perfil.Ativo, permissoes = perfil.Permissoes.Select(p => p.Permissao.Codigo) });

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

        var dadosNovos = JsonSerializer.Serialize(new { nome = perfil.Nome, descricao = perfil.Descricao, ativo = perfil.Ativo, permissoes = perfil.Permissoes.Select(p => p.Permissao.Codigo) });

        var auditoria = new WebApolice.Modulos.Seguranca.Domain.Auditoria.AuditoriaPermissao(
            acao: "PERFIL_ALTERADO",
            entidadeTipo: "PERFIL",
            entidadeId: perfil.Id,
            perfilId: perfil.Id,
            usuarioExecutorId: executorId,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: dadosNovos
        );
        _dbContext.AuditoriaPermissoes.Add(auditoria);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
