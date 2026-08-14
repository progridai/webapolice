using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Domain;
using WebApolice.Modulos.Seguranca.Domain.Exceptions;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Usuarios;

public class AtualizarUsuarioUseCase
{
    private readonly SegurancaDbContext _dbContext;
    private readonly IKeycloakUsuariosAdminClient _keycloakClient;
    private readonly IContextoUsuarioAutenticado _contexto;
    private readonly ILogger<AtualizarUsuarioUseCase> _logger;

    public AtualizarUsuarioUseCase(
        SegurancaDbContext dbContext,
        IKeycloakUsuariosAdminClient keycloakClient,
        IContextoUsuarioAutenticado contexto,
        ILogger<AtualizarUsuarioUseCase> logger)
    {
        _dbContext = dbContext;
        _keycloakClient = keycloakClient;
        _contexto = contexto;
        _logger = logger;
    }

    public async Task ExecuteAsync(
        Guid publicId,
        string nome,
        string email,
        bool ativo,
        List<Guid> perfilPublicIds,
        CancellationToken cancellationToken)
    {
        var usuario = await _dbContext.Usuarios
            .Include(u => u.Perfis)
            .ThenInclude(up => up.Perfil)
            .FirstOrDefaultAsync(u => u.PublicId == publicId, cancellationToken);

        if (usuario == null) throw new UsuarioInvalidoException("Usuário não encontrado.");
        if (string.IsNullOrWhiteSpace(nome)) throw new UsuarioInvalidoException("Nome é obrigatório");
        if (string.IsNullOrWhiteSpace(email)) throw new UsuarioInvalidoException("E-mail é obrigatório");

        var perfisParaAtribuir = await _dbContext.Perfis
            .Where(p => perfilPublicIds.Contains(p.PublicId))
            .ToListAsync(cancellationToken);

        if (perfisParaAtribuir.Any(p => !p.Ativo && !usuario.Perfis.Any(up => up.PerfilId == p.Id)))
        {
            throw new UsuarioInvalidoException("Não é permitido atribuir um perfil inativo que o usuário já não possua.");
        }

        var keycloakAnterior = await _keycloakClient.ObterUsuarioPorSubAsync(usuario.KeycloakSub, cancellationToken);
        if (keycloakAnterior == null) throw new UsuarioInvalidoException("Usuário não encontrado no Keycloak.");

        await _keycloakClient.AtualizarUsuarioAsync(usuario.KeycloakSub, email, nome, ativo, cancellationToken);

        var executor = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.KeycloakSub == _contexto.KeycloakSub, cancellationToken);
        var executorId = executor?.Id;

        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var dadosAnteriores = JsonSerializer.SerializeToDocument(new { nome = usuario.Nome, email = usuario.Email, ativo = usuario.Ativo, perfis = usuario.Perfis.Select(p => p.Perfil.Codigo) });

            usuario.AtualizarAdmin(nome, email, ativo);

            var perfisRemover = usuario.Perfis.Where(up => !perfilPublicIds.Contains(up.Perfil.PublicId)).ToList();
            foreach (var pr in perfisRemover)
            {
                usuario.Perfis.Remove(pr);
            }

            var perfisAdicionar = perfisParaAtribuir.Where(p => !usuario.Perfis.Any(up => up.PerfilId == p.Id)).ToList();
            foreach (var pa in perfisAdicionar)
            {
                usuario.Perfis.Add(new WebApolice.Modulos.Seguranca.Domain.Relacionamentos.UsuarioPerfil(
                    usuarioId: usuario.Id,
                    perfilId: pa.Id,
                    atribuidoPorUsuarioId: executorId
                ));
            }

            var dadosNovos = JsonSerializer.SerializeToDocument(new { nome = usuario.Nome, email = usuario.Email, ativo = usuario.Ativo, perfis = perfisParaAtribuir.Select(p => p.Codigo) });

            var auditoria = new WebApolice.Modulos.Seguranca.Domain.Auditoria.AuditoriaPermissao(
                acao: "USUARIO_ALTERADO",
                entidadeTipo: "USUARIO",
                entidadeId: usuario.Id,
                usuarioAfetadoId: usuario.Id,
                usuarioExecutorId: executorId,
                dadosAnteriores: dadosAnteriores,
                dadosNovos: dadosNovos
            );
            _dbContext.AuditoriaPermissoes.Add(auditoria);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exLocal)
        {
            await transaction.RollbackAsync(cancellationToken);
            try
            {
                using var compensacaoCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                await _keycloakClient.AtualizarUsuarioAsync(usuario.KeycloakSub, keycloakAnterior.Email, $"{keycloakAnterior.FirstName} {keycloakAnterior.LastName}".Trim(), keycloakAnterior.Enabled, compensacaoCts.Token);
            }
            catch (Exception exCompensacao)
            {
                _logger.LogCritical(exCompensacao, "Falha ao restaurar dados do usuário no Keycloak após erro na persistência local. KeycloakSub: {KeycloakSub}", usuario.KeycloakSub);
            }
            throw;
        }
    }
}
