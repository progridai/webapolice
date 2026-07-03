namespace WebApolice.Api.Autenticacao;

/// <summary>
/// Configuração de autenticação JWT Bearer lida a partir do appsettings ou variáveis de ambiente.
/// Nenhum valor padrão sensível é definido aqui — a aplicação falha na inicialização se Authority ou Audience estiverem ausentes.
/// </summary>
public sealed class ConfiguracaoAutenticacao
{
    public const string SecaoNome = "Authentication";

    /// <summary>
    /// URL da authority OpenID Connect (Keycloak realm).
    /// Ex: http://127.0.0.1:8080/realms/webapolice
    /// Pode ser sobrescrita por: Authentication__Authority (variável de ambiente)
    /// </summary>
    public string Authority { get; init; } = string.Empty;

    /// <summary>
    /// Audience esperada no claim 'aud' do access token.
    /// Valor esperado: webapolice-api
    /// Pode ser sobrescrita por: Authentication__Audience (variável de ambiente)
    /// </summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// Se true, exige HTTPS para comunicação com a authority.
    /// Deve ser true em produção. Pode ser false somente em desenvolvimento local.
    /// Pode ser sobrescrita por: Authentication__RequireHttpsMetadata (variável de ambiente)
    /// </summary>
    public bool RequireHttpsMetadata { get; init; } = true;
}
