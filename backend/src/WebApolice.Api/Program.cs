using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebApolice.Api.Autenticacao;
using WebApolice.Api.Autorizacao;
using WebApolice.Api.Infrastructure.Errors;

var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// CONFIGURAÇÃO EXTERNA: Autenticação
// =============================================================================
// Valores lidos de appsettings.json, appsettings.{Environment}.json ou variáveis
// de ambiente no padrão ASP.NET Core:
//   Authentication__Authority
//   Authentication__Audience
//   Authentication__RequireHttpsMetadata
// Nenhuma credencial sensível deve existir em appsettings.json.
// =============================================================================
var configuracaoAuth = builder.Configuration
    .GetSection(ConfiguracaoAutenticacao.SecaoNome)
    .Get<ConfiguracaoAutenticacao>()
    ?? new ConfiguracaoAutenticacao();

// Falhar na inicialização se Authority ou Audience não estiverem configurados
if (string.IsNullOrWhiteSpace(configuracaoAuth.Authority))
{
    throw new InvalidOperationException(
        "A configuração 'Authentication:Authority' é obrigatória e não foi fornecida. " +
        "Defina via appsettings.json, appsettings.{Environment}.json ou pela variável de ambiente " +
        "'Authentication__Authority'.");
}

if (string.IsNullOrWhiteSpace(configuracaoAuth.Audience))
{
    throw new InvalidOperationException(
        "A configuração 'Authentication:Audience' é obrigatória e não foi fornecida. " +
        "Defina via appsettings.json, appsettings.{Environment}.json ou pela variável de ambiente " +
        "'Authentication__Audience'.");
}

// =============================================================================
// AUTENTICAÇÃO JWT BEARER
// =============================================================================
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opcoes =>
    {
        // Authority: URL base do realm Keycloak. O middleware usa o discovery endpoint
        // (.well-known/openid-configuration) para obter automaticamente as chaves JWKS
        // e o issuer esperado. Nenhuma chave pública é fixada manualmente.
        opcoes.Authority = configuracaoAuth.Authority;

        // Validar que o token foi emitido para o client webapolice-api
        opcoes.Audience = configuracaoAuth.Audience;

        // RequireHttpsMetadata = false somente em desenvolvimento local.
        // Em produção, HTTPS é obrigatório sem alteração de código:
        // basta definir Authentication__RequireHttpsMetadata=true via variável de ambiente.
        opcoes.RequireHttpsMetadata = configuracaoAuth.RequireHttpsMetadata;

        opcoes.TokenValidationParameters = new TokenValidationParameters
        {
            // Validações obrigatórias
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            // O issuer e as chaves de assinatura são obtidos automaticamente via
            // metadata da Authority (JWKS). Não fixamos chaves ou issuer manualmente.
            ValidIssuer = configuracaoAuth.Authority,
            ValidAudience = configuracaoAuth.Audience,

            // Mapear o claim 'preferred_username' como o nome do usuário no ASP.NET Core
            NameClaimType = "preferred_username",

            // Tolerância de relógio conservadora (padrão do framework: 5 minutos)
            // Reduzida para 1 minuto para limitar a janela de tokens expirados aceitos
            ClockSkew = TimeSpan.FromMinutes(1),
        };

        // Resposta 401 estruturada em JSON com ProblemDetails
        // Ativada ao receber requisição sem token ou com token inválido
        opcoes.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                // Suprimir a resposta padrão e retornar ProblemDetails estruturado
                context.HandleResponse();

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json; charset=utf-8";

                var traceId = context.HttpContext.TraceIdentifier;
                var problemDetails = new ProblemDetails
                {
                    Type = "https://webapolice/errors/nao-autenticado",
                    Title = "Não autenticado",
                    Status = StatusCodes.Status401Unauthorized,
                    Detail = "É necessário apresentar um token de acesso válido.",
                    Instance = context.Request.Path,
                };
                problemDetails.Extensions["traceId"] = traceId;

                var json = JsonSerializer.Serialize(problemDetails);
                await context.Response.WriteAsync(json);
            },
        };
    });

// =============================================================================
// TRANSFORMADOR DE ROLES DO REALM
// =============================================================================
// Converte realm_access.roles do JWT Keycloak para ClaimTypes.Role do ASP.NET Core
builder.Services.AddScoped<IClaimsTransformation, TransformadorRolesDoRealm>();

// =============================================================================
// AUTORIZAÇÃO COM POLÍTICAS
// =============================================================================
// Respostas 403 estruturadas via ProblemDetails para falhas de autorização
builder.Services.AddAuthorization(opcoes =>
{
    opcoes.AddPolicy(PoliticasAutorizacao.Admin, policy =>
        policy.RequireRole("admin"));

    opcoes.AddPolicy(PoliticasAutorizacao.Gestor, policy =>
        policy.RequireRole("gestor"));

    opcoes.AddPolicy(PoliticasAutorizacao.Operador, policy =>
        policy.RequireRole("operador"));
});

// Suporte a ProblemDetails para respostas 403 estruturadas
builder.Services.AddProblemDetails();

// Tratamento de exceções via IExceptionHandler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// =============================================================================
// OPENAPI & CONTROLLERS
// =============================================================================
builder.Services.AddOpenApi();
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Type = "https://webapolice/errors/requisicao-invalida",
                Title = "Requisição inválida",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Um ou mais erros de validação ocorreram.",
                Instance = context.HttpContext.Request.Path
            };
            problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

            return new BadRequestObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json; charset=utf-8" }
            };
        };
    });

var app = builder.Build();

// =============================================================================
// PIPELINE DE MIDDLEWARE
// =============================================================================

// Tratamento centralizado de exceções não tratadas delegando para o IExceptionHandler registrado
app.UseExceptionHandler();

// Respostas estruturadas para 403 Forbidden (autorização negada)
// Deve ser configurado antes de UseAuthentication e UseAuthorization
app.UseStatusCodePages(async statusCodeContext =>
{
    var context = statusCodeContext.HttpContext;

    // Responder apenas para 403 que ainda não possuem corpo (Content-Length = 0 ou sem body)
    if (context.Response.StatusCode == StatusCodes.Status403Forbidden &&
        !context.Response.HasStarted)
    {
        context.Response.ContentType = "application/problem+json; charset=utf-8";

        var traceId = context.TraceIdentifier;
        var problemDetails = new ProblemDetails
        {
            Type = "https://webapolice/errors/acesso-negado",
            Title = "Acesso negado",
            Status = StatusCodes.Status403Forbidden,
            Detail = "O usuário autenticado não possui permissão para acessar este recurso.",
            Instance = context.Request.Path,
        };
        problemDetails.Extensions["traceId"] = traceId;

        var json = JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(json);
    }
});

// HTTPS somente em produção/homologação (desenvolvimento local e testes usam HTTP)
if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Autenticação e Autorização devem ser adicionados antes dos endpoints protegidos
app.UseAuthentication();
app.UseAuthorization();

// =============================================================================
// ENDPOINTS PÚBLICOS
// =============================================================================

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "healthy",
    application = "WebApolice.Api"
}))
.WithName("GetHealth")
.AllowAnonymous();

app.MapGet("/api/version", (IWebHostEnvironment env) => Results.Ok(new
{
    application = "WebApolice.Api",
    version = "0.1.0",
    environment = env.EnvironmentName
}))
.WithName("GetVersion")
.AllowAnonymous();

if (app.Environment.IsEnvironment("Testing"))
{
    var testingGroup = app.MapGroup("/api/test")
        .AllowAnonymous();

    testingGroup.MapGet("/not-found", () =>
    {
        throw new DemoRecursoNaoEncontradoException("Recurso não encontrado no ambiente de testes.");
    });

    testingGroup.MapGet("/conflict", () =>
    {
        throw new DemoConflitoDeNegocioException("Conflito simulado no ambiente de testes.");
    });

    testingGroup.MapGet("/unprocessable", () =>
    {
        throw new DemoRegraDeNegocioException("Regra de negócio violada no ambiente de testes.");
    });

    testingGroup.MapGet("/internal-error", () =>
    {
        throw new Exception("Erro interno simulado.");
    });

    testingGroup.MapGet("/canceled", () =>
    {
        throw new OperationCanceledException("Operação cancelada.");
    });

    testingGroup.MapGet("/secure", () => "Secure Area").RequireAuthorization();
}

app.MapControllers();

// =============================================================================
// ENDPOINTS DE AUTENTICAÇÃO / AUTORIZAÇÃO (técnicos - não contêm lógica de negócio)
// =============================================================================

// GET /api/auth/me
// Retorna dados não sensíveis do usuário autenticado para validação da integração.
// Requer: autenticação válida (token JWT Bearer)
// Retorna: id (sub), usuario (preferred_username), roles (realm roles mapeadas)
// NÃO retorna: token, refresh token, claims sensíveis, payload JWT completo.
app.MapGet("/api/auth/me", (HttpContext httpContext) =>
{
    var principal = httpContext.User;

    var sub = principal.FindFirst("sub")?.Value
        ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? string.Empty;

    var preferredUsername = principal.FindFirst("preferred_username")?.Value
        ?? principal.Identity?.Name
        ?? string.Empty;

    var roles = principal.Claims
        .Where(c => c.Type == ClaimTypes.Role)
        .Select(c => c.Value)
        .ToList();

    var usuario = new UsuarioAutenticado(sub, preferredUsername, roles);

    return Results.Ok(new
    {
        id = usuario.Id,
        usuario = usuario.Usuario,
        roles = usuario.Roles
    });
})
.WithName("GetMe")
.RequireAuthorization();

// GET /api/admin/ping
// Endpoint técnico de validação exclusivo para administradores.
// Requer: autenticação válida + política Admin (role 'admin')
// Comportamento esperado:
//   - Sem token:               401 Unauthorized
//   - Token sem role 'admin':  403 Forbidden
//   - Token com role 'admin':  200 OK
app.MapGet("/api/admin/ping", () => Results.Ok(new
{
    mensagem = "pong",
    area = "admin"
}))
.WithName("AdminPing")
.RequireAuthorization(PoliticasAutorizacao.Admin);

app.Run();

namespace WebApolice.Api.Infrastructure.Errors
{
    // Classe temporária para testes de model binding
    public class TestInput
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Nome { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        [HttpPost("bad-request")]
        public IActionResult PostBadRequest([FromBody] TestInput input)
        {
            return Ok(input);
        }
    }
}

// Necessário para tornar o Program visível ao projeto de testes de integração (WebApplicationFactory)
public partial class Program { }
