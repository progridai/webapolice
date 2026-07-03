namespace WebApolice.Api.Autorizacao;

/// <summary>
/// Centraliza os nomes das políticas de autorização do WebApólice.
/// Evita strings espalhadas pelos endpoints e garante consistência.
///
/// Políticas desta etapa:
/// - Admin: acesso exclusivo para usuários com role 'admin'
/// - Gestor: acesso exclusivo para usuários com role 'gestor'
/// - Operador: acesso exclusivo para usuários com role 'operador'
///
/// Não há hierarquia implícita entre as políticas nesta versão.
/// Um usuário 'admin' NÃO atende automaticamente às políticas 'gestor' ou 'operador'
/// sem uma decisão formal e documentada.
/// </summary>
public static class PoliticasAutorizacao
{
    public const string Admin = "Politica.Admin";
    public const string Gestor = "Politica.Gestor";
    public const string Operador = "Politica.Operador";
}
