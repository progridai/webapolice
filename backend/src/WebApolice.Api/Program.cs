using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApolice.Api.Autenticacao;
using WebApolice.Shared.Infrastructure.Security;
using WebApolice.Api.Infrastructure.Errors;
using WebApolice.Shared.Infrastructure.Persistence;
using WebApolice.Modulos.Clientes;

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
builder.Services.AddScoped<IClaimsTransformation, TransformadorRolesDoRealm>();

// =============================================================================
// AUTORIZAÇÃO COM POLÍTICAS
// =============================================================================
builder.Services.AddAuthorization(opcoes =>
{
    opcoes.AddPolicy(PoliticasAutorizacao.Administracao, policy =>
        policy.RequireRole(PerfisAcesso.Admin));

    opcoes.AddPolicy(PoliticasAutorizacao.GestaoClientes, policy =>
        policy.RequireRole(PerfisAcesso.Gestor, PerfisAcesso.Admin));

    opcoes.AddPolicy(PoliticasAutorizacao.ConsultaClientes, policy =>
        policy.RequireRole(PerfisAcesso.Operador, PerfisAcesso.Gestor, PerfisAcesso.Admin));
});

// Suporte a ProblemDetails para respostas 403 estruturadas
builder.Services.AddProblemDetails();

// Tratamento de exceções via IExceptionHandler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// =============================================================================
// PERSISTÊNCIA DE DADOS (POSTGRESQL + EF CORE)
// =============================================================================
var connectionString = builder.Configuration.GetConnectionString("PostgreSql");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("A configuração 'ConnectionStrings:PostgreSql' é obrigatória.");
}

builder.Services.AddScoped<System.Data.Common.DbConnection>(sp =>
{
    return new Npgsql.NpgsqlConnection(connectionString);
});

builder.Services.AddDbContext<InfraestruturaDbContext>((sp, options) =>
{
    var connection = sp.GetRequiredService<System.Data.Common.DbConnection>();
    options.UseNpgsql(connection, o => 
           {
               o.MigrationsHistoryTable("__EFMigrationsHistory", "infraestrutura");
           })
           .UseSnakeCaseNamingConvention();
});

// =============================================================================
// AUDITORIA (POSTGRESQL + EF CORE)
// =============================================================================
builder.Services.AddDbContext<WebApolice.Auditoria.Infrastructure.AuditoriaDbContext>((sp, options) =>
{
    var connection = sp.GetRequiredService<System.Data.Common.DbConnection>();
    options.UseNpgsql(connection, o => 
           {
               o.MigrationsHistoryTable("__EFMigrationsHistory", "auditoria");
           })
           .UseSnakeCaseNamingConvention();
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<WebApolice.Auditoria.Contracts.IContextoAuditoria, WebApolice.Api.Infrastructure.ContextoAuditoriaHttp>();
builder.Services.AddScoped<WebApolice.Auditoria.Contracts.IRegistradorAuditoria, WebApolice.Auditoria.Infrastructure.RegistradorAuditoria>();

// =============================================================================
// HEALTH CHECKS
// =============================================================================
builder.Services.AddHealthChecks()
    .AddDbContextCheck<InfraestruturaDbContext>("postgresql");

// Módulo Clientes
builder.Services.AddClientesModule(builder.Configuration);

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

app.MapHealthChecks("/api/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false, // liveness
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(report.Status.ToString());
    }
}).AllowAnonymous();

app.MapHealthChecks("/api/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false, // liveness
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(report.Status.ToString());
    }
}).AllowAnonymous();

app.MapHealthChecks("/api/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    // readiness verifica dependências, sem expor mensagens de erro internas
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new { name = e.Key, status = e.Value.Status.ToString() })
        });
        await context.Response.WriteAsync(result);
    }
}).AllowAnonymous();


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
.RequireAuthorization(PoliticasAutorizacao.Administracao);

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
