using System.Text.Json;
using System.Text.Json.Nodes;
using WebApolice.SharedKernel.Text;

namespace WebApolice.Api.Infrastructure;

/// <summary>Padroniza antes do binding e validação, também para clientes externos à SPA.</summary>
public sealed class PadronizacaoTextoMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.HasJsonContentType() ||
            !(HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method) || HttpMethods.IsPatch(context.Request.Method)))
        {
            await next(context);
            return;
        }
        context.Request.EnableBuffering();
        var original = context.Request.Body;
        var length = context.Request.ContentLength;
        JsonNode? json;
        try { json = await JsonNode.ParseAsync(original, cancellationToken: context.RequestAborted); }
        catch (JsonException)
        {
            original.Position = 0;
            await next(context); // O binding mantém a resposta 400 habitual para JSON inválido.
            return;
        }
        original.Position = 0;
        Normalize(json);
        await using var normalized = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(json));
        context.Request.Body = normalized;
        context.Request.ContentLength = normalized.Length;
        try { await next(context); }
        finally { context.Request.Body = original; context.Request.ContentLength = length; }
    }

    private static void Normalize(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            foreach (var pair in obj.ToArray())
            {
                if (PadronizacaoTexto.CampoHumano(pair.Key) && pair.Value is JsonValue value && value.TryGetValue<string>(out var text))
                    obj[pair.Key] = PadronizacaoTexto.TitleCase(text);
                else Normalize(pair.Value);
            }
        }
        else if (node is JsonArray array)
            foreach (var item in array) Normalize(item);
    }
}
