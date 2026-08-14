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

public class CriarPerfilUseCase
{
    private readonly SegurancaDbContext _dbContext;
    private readonly IContextoUsuarioAutenticado _contexto;

    public CriarPerfilUseCase(SegurancaDbContext dbContext, IContextoUsuarioAutenticado contexto)
    {
        _dbContext = dbContext;
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

        var duplicado = await _dbContext.Perfis.AnyAsync(p => p.Codigo == codigo, cancellationToken);
        if (duplicado) throw new InvalidOperationException("Já existe um perfil com esse código.");

        var permissoesAtribuir = await _dbContext.Permissoes
            .Where(p => permissaoPublicIds.Contains(p.PublicId) && p.Ativo)
            .ToListAsync(cancellationToken);

        if (permissoesAtribuir.Count != permissaoPublicIds.Count)
        {
            throw new InvalidOperationException("Uma ou mais permissões são inválidas ou estão inativas.");
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var executor = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.KeycloakSub == _contexto.KeycloakSub, cancellationToken);
        var executorId = executor?.Id;

        var novoPerfil = new Perfil(codigo, nome, descricao, ativo, false, false);

        _dbContext.Perfis.Add(novoPerfil);
        await _dbContext.SaveChangesAsync(cancellationToken);

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
        _dbContext.AuditoriaPermissoes.Add(auditoria);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return novoPerfil.PublicId;
    }
}
