using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApolice.Modulos.Seguranca.Application.Ports;

public interface IKeycloakUsuariosAdminClient
{
    Task<string> CriarUsuarioAsync(
        string username,
        string email,
        string nome,
        bool ativo,
        CancellationToken cancellationToken);

    Task DefinirSenhaTemporariaAsync(
        string keycloakSub,
        string senhaTemporaria,
        CancellationToken cancellationToken);

    Task AtualizarUsuarioAsync(
        string keycloakSub,
        string email,
        string nome,
        bool ativo,
        CancellationToken cancellationToken);

    Task InativarUsuarioAsync(
        string keycloakSub,
        CancellationToken cancellationToken);

    Task<bool> ExisteUsernameAsync(
        string username,
        CancellationToken cancellationToken);

    Task<bool> ExisteEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task<KeycloakUsuarioRecord?> ObterUsuarioPorSubAsync(
        string keycloakSub,
        CancellationToken cancellationToken);

    Task RemoverUsuarioAsync(
        string keycloakSub,
        CancellationToken cancellationToken);
}

public record KeycloakUsuarioRecord(
    string Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    bool Enabled);
