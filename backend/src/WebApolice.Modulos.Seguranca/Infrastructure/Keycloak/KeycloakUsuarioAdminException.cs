using System.Net;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Keycloak;

/// <summary>
/// Representa uma resposta de erro devolvida pela API administrativa do Keycloak.
/// O corpo original deve ser usado somente para diagnóstico interno e nunca deve
/// ser devolvido diretamente ao cliente da API.
/// </summary>
public sealed class KeycloakUsuarioAdminException : Exception
{
    public KeycloakUsuarioAdminException(
        string message,
        HttpStatusCode statusCode,
        string providerResponse)
        : base(message)
    {
        StatusCode = statusCode;
        ProviderResponse = providerResponse;
    }

    public HttpStatusCode StatusCode { get; }

    public string ProviderResponse { get; }
}
