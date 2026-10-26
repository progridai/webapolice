using System.Net;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace WebApolice.Api.Infrastructure;

public static class SegurancaHttp
{
    public static void Configurar(WebApplicationBuilder builder)
    {
        var proxies = builder.Configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [];
        var enderecos = proxies.Select(p => IPAddress.TryParse(p, out var ip) ? ip :
            throw new InvalidOperationException("ReverseProxy:KnownProxies deve conter somente endereços IP explícitos.")).ToArray();
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = 1;
            options.KnownProxies.Clear();
            options.KnownIPNetworks.Clear();
            foreach (var ip in enderecos) options.KnownProxies.Add(ip);
        });

        builder.Services.AddRateLimiter(options =>
        {
            var limite = builder.Configuration.GetValue("RateLimiting:PermitLimit", 120);
            if (limite < 1) throw new InvalidOperationException("RateLimiting:PermitLimit deve ser positivo.");
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                context.Request.Path.StartsWithSegments("/api/health")
                    ? RateLimitPartition.GetNoLimiter("health")
                    : RateLimitPartition.GetFixedWindowLimiter(
                        context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.User.FindFirstValue("sub") ??
                        context.Connection.RemoteIpAddress?.ToString() ?? "origem-desconhecida",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = limite, Window = TimeSpan.FromMinutes(1), QueueLimit = 0,
                            AutoReplenishment = true
                        }));
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Limite de requisições excedido",
                    Detail = "Aguarde um minuto e tente novamente.",
                    Extensions = { ["traceId"] = context.HttpContext.TraceIdentifier }
                }, options: (System.Text.Json.JsonSerializerOptions?)null,
                    contentType: "application/problem+json", cancellationToken: token);
            };
        });
        builder.Services.AddHsts(options => options.MaxAge = TimeSpan.FromDays(30));
    }

    public static void UsarProxyConfiavel(WebApplication app)
    {
        // Com lista vazia, nenhum cabeçalho de proxy é interpretado.
        if ((app.Configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? []).Length > 0)
            app.UseForwardedHeaders();
    }
}
