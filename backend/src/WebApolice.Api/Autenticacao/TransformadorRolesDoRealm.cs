using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace WebApolice.Api.Autenticacao;

/// <summary>
/// Transforma claims do JWT do Keycloak para que as realm roles de
/// <c>realm_access.roles</c> sejam reconhecidas como claims de role padrão do ASP.NET Core.
///
/// O Keycloak entrega roles globais no formato:
/// <code>
/// {
///   "realm_access": {
///     "roles": ["admin", "gestor", "operador", ...]
///   }
/// }
/// </code>
///
/// O ASP.NET Core não interpreta esse objeto aninhado automaticamente.
/// Esta classe lê o JSON do claim e adiciona cada role como um <c>ClaimTypes.Role</c>
/// separado, garantindo que [Authorize(Roles = "admin")] e as políticas funcionem.
///
/// Requisitos de segurança implementados:
/// - Roles não são duplicadas caso a transformação seja executada mais de uma vez.
/// - Claim ausente é tratado com segurança (sem role concedida).
/// - JSON inválido não derruba a aplicação (sem role concedida).
/// - Claims originais são preservados.
/// </summary>
public sealed class TransformadorRolesDoRealm : IClaimsTransformation
{
    private const string ClaimRealmAccess = "realm_access";

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Verificar se o usuário está autenticado antes de transformar
        if (principal.Identity is not { IsAuthenticated: true })
        {
            return Task.FromResult(principal);
        }

        var realmAccessClaim = principal.FindFirst(ClaimRealmAccess);
        if (realmAccessClaim is null)
        {
            return Task.FromResult(principal);
        }

        List<string> roles;
        try
        {
            var realmAccess = JsonDocument.Parse(realmAccessClaim.Value);
            if (!realmAccess.RootElement.TryGetProperty("roles", out var rolesElement))
            {
                return Task.FromResult(principal);
            }

            roles = new List<string>();
            foreach (var roleElement in rolesElement.EnumerateArray())
            {
                var roleName = roleElement.GetString();
                if (!string.IsNullOrWhiteSpace(roleName))
                {
                    roles.Add(roleName);
                }
            }
        }
        catch (JsonException)
        {
            // JSON inválido: não concede nenhuma role, não derruba a aplicação
            return Task.FromResult(principal);
        }

        if (roles.Count == 0)
        {
            return Task.FromResult(principal);
        }

        // Evitar duplicação: só adicionar claims que ainda não existem como ClaimTypes.Role
        var identidadeAtual = (ClaimsIdentity)principal.Identity!;
        var rolesExistentes = identidadeAtual.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToHashSet(StringComparer.Ordinal);

        var claimsParaAdicionar = roles
            .Where(r => !rolesExistentes.Contains(r))
            .Select(r => new Claim(ClaimTypes.Role, r))
            .ToList();

        if (claimsParaAdicionar.Count == 0)
        {
            return Task.FromResult(principal);
        }

        // Clonar a identidade para não mutar o principal original (thread-safety)
        var novaIdentidade = identidadeAtual.Clone();
        novaIdentidade.AddClaims(claimsParaAdicionar);

        var novoPrincipal = new ClaimsPrincipal(novaIdentidade);
        return Task.FromResult(novoPrincipal);
    }
}
