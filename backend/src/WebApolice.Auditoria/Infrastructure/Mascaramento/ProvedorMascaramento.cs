using System.Text.Json;
using System.Text.Json.Nodes;
using WebApolice.Auditoria.Domain.Exceptions;

namespace WebApolice.Auditoria.Infrastructure.Mascaramento;

public class ProvedorMascaramento
{
    private static readonly HashSet<string> ChavesProibidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "senha", "password", "token", "access_token", "refresh_token", 
        "secret", "client_secret", "authorization", "connection_string", 
        "chave_pix", "numero_cartao", "codigo_seguranca", "cvv",
        "chave_privada", "private_key", "api_key"
    };

    public static JsonDocument? ValidarERejeitarSegredos(JsonDocument? jsonOriginal)
    {
        if (jsonOriginal == null)
            return null;

        var jsonNode = JsonNode.Parse(jsonOriginal.RootElement.GetRawText());
        if (jsonNode == null) return jsonOriginal;

        var chavesEncontradas = new List<string>();
        ValidarNo(jsonNode, chavesEncontradas, 0);

        if (chavesEncontradas.Any())
        {
            throw new ValidacaoAuditoriaException($"Tentativa de auditar dados contendo segredos ou credenciais não permitidos. Chaves encontradas: {string.Join(", ", chavesEncontradas.Distinct())}");
        }

        return jsonOriginal;
    }

    private static void ValidarNo(JsonNode node, List<string> chavesEncontradas, int profundidade)
    {
        if (profundidade > 20)
        {
            throw new ValidacaoAuditoriaException("Profundidade máxima de JSON aninhado excedida (20 níveis).");
        }

        if (node is JsonObject obj)
        {
            foreach (var kvp in obj)
            {
                if (ChavesProibidas.Contains(kvp.Key))
                {
                    chavesEncontradas.Add(kvp.Key);
                }
                else if (kvp.Value != null)
                {
                    ValidarNo(kvp.Value, chavesEncontradas, profundidade + 1);
                }
            }
        }
        else if (node is JsonArray arr)
        {
            foreach (var item in arr)
            {
                if (item != null)
                {
                    ValidarNo(item, chavesEncontradas, profundidade + 1);
                }
            }
        }
    }
}
