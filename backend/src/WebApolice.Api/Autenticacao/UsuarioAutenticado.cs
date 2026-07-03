namespace WebApolice.Api.Autenticacao;

/// <summary>
/// Representa os dados do usuário autenticado disponíveis no contexto da requisição.
///
/// Mapeamento de claims:
/// - <see cref="Id"/>: claim 'sub' — identificador externo imutável do usuário no Keycloak.
///   Deve ser usado como chave de referência ao usuário no sistema.
/// - <see cref="Usuario"/>: claim 'preferred_username' — nome de login ou exibição.
///   Não deve ser usado como identificador persistente pois pode mudar.
/// - <see cref="Roles"/>: roles do realm extraídas de 'realm_access.roles' pelo
///   <see cref="TransformadorRolesDoRealm"/>.
///
/// Esta representação é intencional e minimalista para esta etapa.
/// Não cria serviço genérico de usuário, contexto complexo, nem conecta ao banco de dados.
/// </summary>
public sealed record UsuarioAutenticado(
    string Id,
    string Usuario,
    IReadOnlyList<string> Roles
);
