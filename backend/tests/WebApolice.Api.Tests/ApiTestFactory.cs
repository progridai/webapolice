using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace WebApolice.Api.Tests;

/// <summary>
/// WebApplicationFactory para testes de integração da API.
///
/// Material criptográfico exclusivo para testes automatizados:
/// ATENÇÃO: A chave RSA gerada aqui existe SOMENTE em memória durante os testes.
/// Ela NÃO é a chave de produção ou de desenvolvimento.
/// Nunca deve ser reutilizada fora do contexto de testes.
///
/// O objetivo é permitir que os testes assinem e validem tokens JWT de forma
/// completamente isolada do Keycloak local — sem depender do servidor estar em execução.
/// </summary>
public sealed class ApiTestFactory : WebApplicationFactory<Program>
{
    // Issuer e Audience fixos para os testes — devem coincidir com o que a API espera
    public const string IsserDeTeste = "http://teste.keycloak.local/realms/webapolice";
    public const string AudienciaDeTeste = "webapolice-api";

    // Chave RSA gerada uma única vez para toda a vida do factory.
    private static readonly RsaSecurityKey ChaveRsaDeTeste = CriarChaveRsaDeTeste();

    static ApiTestFactory()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__PostgreSql", "Host=localhost;Port=5432;Database=test;Username=test;Password=test");
    }

    private static RsaSecurityKey CriarChaveRsaDeTeste()
    {
        var rsa = RSA.Create(2048);
        return new RsaSecurityKey(rsa)
        {
            KeyId = "teste-kid-01"
        };
    }

    /// <summary>
    /// Gera um JWT assinado com a chave RSA de testes, com claims e roles customizáveis.
    /// </summary>
    public static string GerarTokenDeTeste(
        string sub = "usuario-de-teste-id",
        string preferredUsername = "usuario.teste",
        IEnumerable<string>? roles = null,
        string? issuer = null,
        string? audience = null,
        DateTime? expiracao = null,
        bool semAssinatura = false)
    {
        var agora = DateTime.UtcNow;
        var realmRoles = roles?.ToList() ?? [];

        // Construir o claim realm_access.roles no mesmo formato que o Keycloak produz
        var realmAccessJson = $$"""{"roles":[{{string.Join(",", realmRoles.Select(r => $"\"{r}\""))}}]}""";

        var claims = new List<Claim>
        {
            new("sub", sub),
            new("preferred_username", preferredUsername),
            new("realm_access", realmAccessJson),
            new("azp", "webapolice-web"),
        };

        if (semAssinatura)
        {
            // Token sem assinatura válida (alg=none) — para teste de rejeição
            var tokenSemAssinatura = new JwtSecurityToken(
                issuer: issuer ?? IsserDeTeste,
                audience: audience ?? AudienciaDeTeste,
                claims: claims,
                notBefore: agora,
                expires: expiracao ?? agora.AddMinutes(5));

            return new JwtSecurityTokenHandler().WriteToken(tokenSemAssinatura);
        }

        var credenciais = new SigningCredentials(ChaveRsaDeTeste, SecurityAlgorithms.RsaSha256);

        // Se expiracao estiver no passado, colocar notBefore também no passado
        var notBefore = expiracao.HasValue && expiracao.Value < agora
            ? expiracao.Value.AddMinutes(-5)
            : agora;

        var token = new JwtSecurityToken(
            issuer: issuer ?? IsserDeTeste,
            audience: audience ?? AudienciaDeTeste,
            claims: claims,
            notBefore: notBefore,
            expires: expiracao ?? agora.AddMinutes(5),
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Usar environment de testes para não tentar se conectar ao Keycloak real
        builder.UseEnvironment("Testing");

        // Definir as configurações mínimas necessárias para a inicialização da API
        // (evitar falha de startup por Authority/Audience ausentes)
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new KeyValuePair<string, string?>[]
            {
                new("Authentication:Authority", IsserDeTeste),
                new("Authentication:Audience", AudienciaDeTeste),
                new("Authentication:RequireHttpsMetadata", "false"),
                new("ConnectionStrings:PostgreSql", "Host=localhost;Port=5432;Database=test;Username=test;Password=test")
            });
        });

        builder.ConfigureServices(services =>
        {
            // Substituir apenas os parâmetros de validação do token JWT, mantendo
            // os event handlers (OnChallenge, etc.) configurados no Program.cs.
            // Usamos IPostConfigureOptions para sobrescrever APÓS a configuração original,
            // mantendo o OnChallenge handler e apenas mudando o IssuerSigningKey e ValidIssuer.
            services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>>(
                new PostConfigureJwtBearerParaTesteOptions(ChaveRsaDeTeste, IsserDeTeste, AudienciaDeTeste));
        });
    }

    /// <summary>
    /// IPostConfigureOptions que substitui apenas os parâmetros de validação do JWT.
    /// Preserva os event handlers (OnChallenge, OnForbidden) do Program.cs.
    /// </summary>
    private sealed class PostConfigureJwtBearerParaTesteOptions
        : IPostConfigureOptions<JwtBearerOptions>
    {
        private readonly RsaSecurityKey _chave;
        private readonly string _issuer;
        private readonly string _audience;

        public PostConfigureJwtBearerParaTesteOptions(
            RsaSecurityKey chave,
            string issuer,
            string audience)
        {
            _chave = chave;
            _issuer = issuer;
            _audience = audience;
        }

        public void PostConfigure(string? name, JwtBearerOptions options)
        {
            // Só aplicar à configuração do scheme JwtBearer padrão
            if (name != JwtBearerDefaults.AuthenticationScheme)
            {
                return;
            }

            // Desabilitar o carregamento de metadata remota (não há Keycloak nos testes)
            options.Authority = null;
            options.RequireHttpsMetadata = false;
            options.Audience = _audience;
            options.ConfigurationManager = null;

            // Substituir os parâmetros de validação para usar a chave de teste
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = _issuer,
                ValidAudience = _audience,

                IssuerSigningKey = _chave,

                NameClaimType = "preferred_username",
                ClockSkew = TimeSpan.Zero, // Zero para testes de expiração precisos
            };
        }
    }
}
