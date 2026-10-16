using System.Globalization;
using System.Text.RegularExpressions;

namespace WebApolice.SharedKernel.Text;

/// <summary>Política única de textos humanos. Não aplicar a códigos, credenciais ou dados originais de auditoria.</summary>
public static partial class PadronizacaoTexto
{
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("pt-BR");
    private static readonly HashSet<string> Campos = new(StringComparer.OrdinalIgnoreCase)
    {
        "Nome", "NomeFantasia", "RazaoSocial", "NomeConjuge", "NomeEmpresa", "NomeReduzido", "NomeFormatado", "Descricao", "Observacao", "Titulo", "Logradouro", "Complemento", "Bairro", "Departamento", "Contato", "Empresa", "Equipe", "Funcao", "Setor", "Motivo", "Causa", "Instituidor", "Instituto", "EstNome", "EstCidade", "EstEndereco", "EstComplemento", "EstBairro"
    };

    private static readonly HashSet<string> PalavrasMinusculas = new(StringComparer.Ordinal)
    {
        "a", "o", "as", "os", "um", "uma", "uns", "umas", "de", "da", "do", "das", "dos", "em", "na", "no", "nas", "nos", "por", "para", "com", "sem", "sob", "sobre", "entre", "até", "ante", "após", "contra", "desde", "perante", "trás", "durante", "mediante", "e", "ou", "nem", "mas", "porém", "contudo", "todavia", "entretanto", "pois", "porque", "portanto", "logo", "que", "se", "como", "quando", "enquanto", "embora", "caso", "conquanto", "senão", "à", "às", "ao", "aos", "pelo", "pela", "pelos", "pelas", "num", "numa", "nuns", "numas", "d"
    };

    public static bool CampoHumano(string campo) => Campos.Contains(campo.Replace("_", ""));

    // Lower antes da capitalização evita a preservação de palavras inteiras em CAIXA ALTA.
    // Mesma separação por letras no frontend e no SQL, incluindo hífens e apóstrofos.
    public static string? TitleCase(string? valor) => valor is null ? null :
        Palavras().Replace(valor.ToLower(Cultura), m => PalavrasMinusculas.Contains(m.Value) ? m.Value : Cultura.TextInfo.ToUpper(m.Value[..1]) + m.Value[1..]);

    [GeneratedRegex(@"\p{L}[\p{L}\p{M}]*")]
    private static partial Regex Palavras();
}
