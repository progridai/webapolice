using System.Security.Claims;
using WebApolice.Api.Autenticacao;
using Xunit;

namespace WebApolice.Api.Tests;

/// <summary>
/// Testes unitários para <see cref="TransformadorRolesDoRealm"/>.
/// Verifica o mapeamento correto de realm_access.roles para ClaimTypes.Role,
/// comportamento com dados ausentes ou inválidos, e ausência de duplicação.
/// </summary>
public sealed class TransformadorRolesDoRealmTests
{
    private readonly TransformadorRolesDoRealm _transformador = new();

    private static ClaimsPrincipal CriarPrincipalAutenticado(IEnumerable<Claim> claims)
    {
        var identidade = new ClaimsIdentity(claims, authenticationType: "jwt");
        return new ClaimsPrincipal(identidade);
    }

    private static ClaimsPrincipal CriarPrincipalNaoAutenticado()
    {
        var identidade = new ClaimsIdentity(); // Sem authenticationType = não autenticado
        return new ClaimsPrincipal(identidade);
    }

    [Fact]
    public async Task Transform_ConverteRolesDoRealmParaClaimTypeRole()
    {
        var realmAccess = """{"roles":["admin","gestor","operador"]}""";
        var claims = new[] { new Claim("realm_access", realmAccess) };
        var principal = CriarPrincipalAutenticado(claims);

        var resultado = await _transformador.TransformAsync(principal);

        var roles = resultado.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        Assert.Contains("admin", roles);
        Assert.Contains("gestor", roles);
        Assert.Contains("operador", roles);
    }

    [Fact]
    public async Task Transform_NaoAdicionaRolesDuplicadas()
    {
        var realmAccess = """{"roles":["admin"]}""";
        var claimsExistentes = new[]
        {
            new Claim("realm_access", realmAccess),
            new Claim(ClaimTypes.Role, "admin"), // Role já existente
        };
        var principal = CriarPrincipalAutenticado(claimsExistentes);

        var resultado = await _transformador.TransformAsync(principal);

        var rolesAdmin = resultado.Claims
            .Where(c => c.Type == ClaimTypes.Role && c.Value == "admin")
            .ToList();

        Assert.Single(rolesAdmin); // Não deve duplicar
    }

    [Fact]
    public async Task Transform_SemClaimRealmAccess_RetornaPrincipalSemAlteracoes()
    {
        var principal = CriarPrincipalAutenticado(new[] { new Claim("sub", "usuario-id") });

        var resultado = await _transformador.TransformAsync(principal);

        var roles = resultado.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        Assert.Empty(roles);
    }

    [Fact]
    public async Task Transform_ComJsonInvalido_NaoConcedRole()
    {
        var claims = new[] { new Claim("realm_access", "isso-nao-e-json-valido") };
        var principal = CriarPrincipalAutenticado(claims);

        var resultado = await _transformador.TransformAsync(principal);

        var roles = resultado.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        Assert.Empty(roles);
    }

    [Fact]
    public async Task Transform_ComJsonInvalido_NaoLancaExcecao()
    {
        var claims = new[] { new Claim("realm_access", "{invalido}") };
        var principal = CriarPrincipalAutenticado(claims);

        // Não deve lançar exceção
        var exception = await Record.ExceptionAsync(() => _transformador.TransformAsync(principal));
        Assert.Null(exception);
    }

    [Fact]
    public async Task Transform_ComRealmAccessSemPropriedadeRoles_NaoConcedRole()
    {
        // JSON válido mas sem a propriedade "roles"
        var claims = new[] { new Claim("realm_access", """{"outras_coisas": "valor"}""") };
        var principal = CriarPrincipalAutenticado(claims);

        var resultado = await _transformador.TransformAsync(principal);

        var roles = resultado.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        Assert.Empty(roles);
    }

    [Fact]
    public async Task Transform_ComArrayRolesVazio_RetornaSemRoles()
    {
        var claims = new[] { new Claim("realm_access", """{"roles":[]}""") };
        var principal = CriarPrincipalAutenticado(claims);

        var resultado = await _transformador.TransformAsync(principal);

        var roles = resultado.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        Assert.Empty(roles);
    }

    [Fact]
    public async Task Transform_PreservaClainsOriginais()
    {
        var realmAccess = """{"roles":["admin"]}""";
        var claims = new[]
        {
            new Claim("sub", "usuario-id-12345"),
            new Claim("preferred_username", "usuario.teste"),
            new Claim("realm_access", realmAccess),
        };
        var principal = CriarPrincipalAutenticado(claims);

        var resultado = await _transformador.TransformAsync(principal);

        // Claims originais devem estar presentes
        Assert.Equal("usuario-id-12345", resultado.FindFirst("sub")?.Value);
        Assert.Equal("usuario.teste", resultado.FindFirst("preferred_username")?.Value);
        Assert.NotNull(resultado.FindFirst("realm_access"));
    }

    [Fact]
    public async Task Transform_UsuarioNaoAutenticado_RetornaPrincipalSemAlteracoes()
    {
        var principal = CriarPrincipalNaoAutenticado();

        var resultado = await _transformador.TransformAsync(principal);

        Assert.False(resultado.Identity?.IsAuthenticated ?? true);
        var roles = resultado.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        Assert.Empty(roles);
    }

    [Fact]
    public async Task Transform_TransformacaoDuasVezes_NaoDuplicaRoles()
    {
        var realmAccess = """{"roles":["gestor"]}""";
        var claims = new[] { new Claim("realm_access", realmAccess) };
        var principal = CriarPrincipalAutenticado(claims);

        // Aplicar transformação duas vezes (simula múltiplas chamadas)
        var resultado1 = await _transformador.TransformAsync(principal);
        var resultado2 = await _transformador.TransformAsync(resultado1);

        var roles = resultado2.Claims
            .Where(c => c.Type == ClaimTypes.Role && c.Value == "gestor")
            .ToList();

        Assert.Single(roles);
    }
}
