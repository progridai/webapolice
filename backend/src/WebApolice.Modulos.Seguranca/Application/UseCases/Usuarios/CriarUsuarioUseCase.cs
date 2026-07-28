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
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Usuarios;

public class CriarUsuarioUseCase
{
    private readonly SegurancaDbContext _dbContext;
    private readonly IKeycloakUsuariosAdminClient _keycloakClient;
    private readonly IContextoUsuarioAutenticado _contexto;
    private readonly ILogger<CriarUsuarioUseCase> _logger;

    public CriarUsuarioUseCase(
        SegurancaDbContext dbContext,
        IKeycloakUsuariosAdminClient keycloakClient,
        IContextoUsuarioAutenticado contexto,
        ILogger<CriarUsuarioUseCase> logger)
    {
        _dbContext = dbContext;
        _keycloakClient = keycloakClient;
        _contexto = contexto;
        _logger = logger;
    }

    public async Task<Guid> ExecuteAsync(
        string username,
        string nome,
        string email,
        string senhaTemporaria,
        bool ativo,
        List<Guid> perfilPublicIds,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username é obrigatório");
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Nome é obrigatório");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("E-mail é obrigatório");
        if (string.IsNullOrWhiteSpace(senhaTemporaria)) throw new ArgumentException("Senha temporária é obrigatória");

        var perfisParaAtribuir = await _dbContext.Perfis
            .Where(p => perfilPublicIds.Contains(p.PublicId) && p.Ativo)
            .ToListAsync(cancellationToken);

        if (perfisParaAtribuir.Count != perfilPublicIds.Count)
        {
            throw new InvalidOperationException("Um ou mais perfis informados são inválidos ou estão inativos.");
        }

        var duplicado = await _dbContext.Usuarios.AnyAsync(u => u.Username == username || u.Email == email, cancellationToken);
        if (duplicado) throw new InvalidOperationException("Username ou E-mail já cadastrado localmente.");

        if (await _keycloakClient.ExisteUsernameAsync(username, cancellationToken))
            throw new InvalidOperationException("Username já existe no Keycloak.");

        if (await _keycloakClient.ExisteEmailAsync(email, cancellationToken))
            throw new InvalidOperationException("E-mail já existe no Keycloak.");

        var keycloakSub = await _keycloakClient.CriarUsuarioAsync(username, email, nome, ativo, cancellationToken);

        try
        {
            await _keycloakClient.DefinirSenhaTemporariaAsync(keycloakSub, senhaTemporaria, cancellationToken);
        }
        catch (Exception exSenha)
        {
            try
            {
                using var compensacaoCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                await _keycloakClient.RemoverUsuarioAsync(keycloakSub, compensacaoCts.Token);
            }
            catch (Exception exCompensacao)
            {
                _logger.LogCritical(exCompensacao, "Falha ao compensar a criação do usuário no Keycloak após erro de senha. KeycloakSub: {KeycloakSub}", keycloakSub);
            }
            throw;
        }

        var executor = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.KeycloakSub == _contexto.KeycloakSub, cancellationToken);
        var executorId = executor?.Id;

        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var novoUsuario = new Usuario(keycloakSub, username, nome, email, ativo);

            _dbContext.Usuarios.Add(novoUsuario);
            await _dbContext.SaveChangesAsync(cancellationToken);

            foreach (var perfil in perfisParaAtribuir)
            {
                novoUsuario.Perfis.Add(new WebApolice.Modulos.Seguranca.Domain.Relacionamentos.UsuarioPerfil(
                    usuarioId: novoUsuario.Id,
                    perfilId: perfil.Id,
                    atribuidoPorUsuarioId: executorId
                ));
            }

            var auditoria = new WebApolice.Modulos.Seguranca.Domain.Auditoria.AuditoriaPermissao(
                acao: "USUARIO_CRIADO",
                entidadeTipo: "USUARIO",
                entidadeId: novoUsuario.Id,
                usuarioAfetadoId: novoUsuario.Id,
                usuarioExecutorId: executorId,
                dadosNovos: JsonSerializer.Serialize(new { username, nome, email, ativo, perfis = perfisParaAtribuir.Select(p => p.Codigo) })
            );
            _dbContext.AuditoriaPermissoes.Add(auditoria);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return novoUsuario.PublicId;
        }
        catch (Exception exLocal)
        {
            await transaction.RollbackAsync(cancellationToken);
            try
            {
                using var compensacaoCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                await _keycloakClient.RemoverUsuarioAsync(keycloakSub, compensacaoCts.Token);
            }
            catch (Exception exCompensacao)
            {
                _logger.LogCritical(exCompensacao, "Falha ao compensar a criação do usuário no Keycloak após erro na persistência local. KeycloakSub: {KeycloakSub}", keycloakSub);
            }
            throw;
        }
    }
}
