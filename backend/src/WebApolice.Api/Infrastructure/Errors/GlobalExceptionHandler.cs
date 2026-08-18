using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;
using WebApolice.Modulos.Seguranca.Domain.Exceptions;

namespace WebApolice.Api.Infrastructure.Errors;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException)
        {
            _logger.LogInformation("Operação cancelada pelo cliente. TraceId: {TraceId}", httpContext.TraceIdentifier);
            httpContext.Response.StatusCode = 499; // Client Closed Request
            return true;
        }

        var problemDetails = CriarProblemDetails(httpContext, exception);

        RegistrarLog(httpContext.TraceIdentifier, exception, problemDetails);

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json; charset=utf-8";

        var json = System.Text.Json.JsonSerializer.Serialize(problemDetails);
        await httpContext.Response.WriteAsync(json, cancellationToken);

        return true;
    }

    private ProblemDetails CriarProblemDetails(HttpContext context, Exception exception)
    {
        var problemDetails = exception switch
        {
            DemoRecursoNaoEncontradoException => new ProblemDetails
            {
                Type = "https://webapolice/errors/recurso-nao-encontrado",
                Title = "Recurso não encontrado",
                Status = StatusCodes.Status404NotFound,
                Detail = exception.Message,
                Instance = context.Request.Path
            },
            DemoConflitoDeNegocioException => new ProblemDetails
            {
                Type = "https://webapolice/errors/conflito",
                Title = "Conflito",
                Status = StatusCodes.Status409Conflict,
                Detail = exception.Message,
                Instance = context.Request.Path
            },
            DemoRegraDeNegocioException => new ProblemDetails
            {
                Type = "https://webapolice/errors/regra-de-negocio",
                Title = "Regra de negócio violada",
                Status = StatusCodes.Status422UnprocessableEntity,
                Detail = exception.Message,
                Instance = context.Request.Path
            },
            ClienteNaoEncontradoException => new ProblemDetails
            {
                Type = "https://webapolice/errors/recurso-nao-encontrado",
                Title = "Recurso não encontrado",
                Status = StatusCodes.Status404NotFound,
                Detail = exception.Message,
                Instance = context.Request.Path
            },
            ClienteJaCadastradoException => new ProblemDetails
            {
                Type = "https://webapolice/errors/conflito",
                Title = "Conflito",
                Status = StatusCodes.Status409Conflict,
                Detail = exception.Message,
                Instance = context.Request.Path
            },
            ClienteInvalidoException => new ProblemDetails
            {
                Type = "https://webapolice/errors/regra-de-negocio",
                Title = "Regra de negócio violada",
                Status = StatusCodes.Status422UnprocessableEntity,
                Detail = exception.Message,
                Instance = context.Request.Path
            },
            UsuarioInvalidoException => new ProblemDetails
            {
                Type = "https://webapolice/errors/regra-de-negocio",
                Title = "Regra de negócio violada",
                Status = StatusCodes.Status422UnprocessableEntity,
                Detail = exception.Message,
                Instance = context.Request.Path
            },
            _ => new ProblemDetails
            {
                Type = "https://webapolice/errors/erro-interno",
                Title = "Ocorreu um erro inesperado no servidor.",
                Status = StatusCodes.Status500InternalServerError,
                Detail = exception.ToString(),
                Instance = context.Request.Path
            }
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        return problemDetails;
    }

    private void RegistrarLog(string traceId, Exception exception, ProblemDetails problemDetails)
    {
        if (problemDetails.Status is >= 400 and < 500)
        {
            _logger.LogWarning(exception, "Erro esperado ocorrido: {Type}. Status: {Status}. TraceId: {TraceId}", problemDetails.Type, problemDetails.Status, traceId);
        }
        else
        {
            _logger.LogError(exception, "Erro inesperado ocorrido. TraceId: {TraceId}", traceId);
        }
    }
}
