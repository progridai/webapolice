using System.Text.RegularExpressions;

record TelefoneParte(int Ordem, string Trecho, string Valor, string Normalizado, string Tipo, string Situacao);

static class TelefoneParserV2
{
    static readonly HashSet<string> Ddds = "11 12 13 14 15 16 17 18 19 21 22 24 27 28 31 32 33 34 35 37 38 41 42 43 44 45 46 47 48 49 51 53 54 55 61 62 63 64 65 66 67 68 69 71 73 74 75 77 79 81 82 83 84 85 86 87 88 89 91 92 93 94 95 96 97 98 99".Split(' ').ToHashSet();
    public static string Digits(string s) => Regex.Replace(s, "[^0-9]", "");
    static string? Ddd(string s)
    {
        if (!Regex.IsMatch(s, @"^[\s()0-9.\-]+$")) return null;
        var n = Digits(s).TrimStart('0');
        return n.Length == 2 && Ddds.Contains(n) ? n : null;
    }
    static string? Completo(string s)
    {
        // An annotation following the number is retained in evidence, never interpreted as digits.
        var numeric = Regex.Match(s.Trim(), @"[+(]?[0-9][0-9\s().*+\-]*").Value.Trim();
        if (numeric.Length == 0 || Digits(s) != Digits(numeric)) return null;
        var d = Digits(numeric);
        if (numeric.Contains('+') && (!numeric.StartsWith("+55") || d.Length is not (12 or 13))) return null;
        var options = new HashSet<string>();
        void Add(string n)
        {
            if (n.Length is not (10 or 11) || !Ddds.Contains(n[..2])) return;
            if ((n.Length == 10 && n[2] is >= '2' and <= '5') || (n.Length == 11 && n[2] == '9')) options.Add(n);
        }
        Add(d);
        if (d.StartsWith('0')) Add(d[1..]);
        if (d.StartsWith("00")) Add(d[2..]);
        if (d.StartsWith("55") && d.Length is 12 or 13) Add(d[2..]);
        if (d.StartsWith("0055") && d.Length is 14 or 15) Add(d[4..]);
        return options.Count == 1 ? options.Single() : null;
    }
    static IEnumerable<string> SepararEspacos(string s)
    {
        // Split only at existing boundaries, with at least seven digits on both sides.
        // Never cut an overlong uninterrupted digit sequence into invented numbers.
        foreach (Match m in Regex.Matches(s, @"\s+-\s+|\s{1,}|-(?=[(0-9])"))
        {
            var left = s[..m.Index].Trim();
            var right = s[(m.Index + m.Length)..].Trim();
            var nl = Digits(left).Length;
            if (nl < 7 || nl > 13 || Digits(right).Length < 7) continue;
            if (Regex.IsMatch(right[..Math.Min(right.Length, 1)], "[A-Za-zÀ-ÿ]")) continue;
            if (Completo(left) is null && (Regex.IsMatch(left, "[A-Za-zÀ-ÿ]") || nl is not (7 or 8 or 9 or 10))) continue;
            yield return left;
            foreach (var part in SepararEspacos(right)) yield return part;
            yield break;
        }
        yield return s;
    }
    public static List<TelefoneParte> Parse(string raw, string originalType = "TELEFONE")
    {
        if (Digits(raw).Length == 0) return [new(1, raw, raw, "", originalType, "SEM_NUMERO")];
        var chunks = Regex.Split(raw, @"[/;|,\\\r\n]+").Where(s => !string.IsNullOrWhiteSpace(s)).SelectMany(s => SepararEspacos(s.Trim())).ToList();
        var result = new List<TelefoneParte>();
        var leadingEvidence = "";
        for (var i = 0; i < chunks.Count; i++)
        {
            var part = chunks[i];
            if (Digits(part).Length == 0)
            {
                if (result.Count > 0) result[^1] = result[^1] with { Trecho = result[^1].Trecho + " / " + part };
                else leadingEvidence += part + " / ";
                continue;
            }
            var ddd = Ddd(part);
            // Associate only an explicit standalone DDD with its immediate local number.
            if (ddd is not null && i + 1 < chunks.Count && Digits(chunks[i + 1]).Length is >= 7 and <= 9)
            {
                part = part.Trim() + " " + chunks[++i].Trim();
            }
            var number = Completo(part);
            var digits = Digits(part);
            if (number is not null)
            {
                var type = number.Length == 10 ? "TELEFONE" : "CELULAR";
                var formatted = $"({number[..2]}) {number[2..^4]}-{number[^4..]}";
                result.Add(new(result.Count + 1, leadingEvidence + part, formatted, number, type, "COMPLETO"));
            }
            else
            {
                // Preserve incomplete digits and annotations. Do not manufacture missing prefixes.
                result.Add(new(result.Count + 1, leadingEvidence + part, part, digits, originalType, "INCOMPLETO"));
            }
            leadingEvidence = "";
        }
        if (result.Count == 0) result.Add(new(1, raw, raw, Digits(raw), originalType, "SEM_NUMERO"));
        if (string.Concat(result.Select(p => Digits(p.Trecho))) != Digits(raw))
            throw new Exception("Separacao V2 perdeu ou duplicou digitos da evidencia original.");
        return result;
    }
    public static void Test()
    {
        var tests = new (string Input, string[] Values, string[] Status)[] {
            ("51 / 32419416/ 51/ 32492519/ 51/ 32466617/51/999861341", ["(51) 3241-9416","(51) 3249-2519","(51) 3246-6617","(51) 99986-1341"], ["COMPLETO","COMPLETO","COMPLETO","COMPLETO"]),
            ("33332364", ["33332364"], ["INCOMPLETO"]),
            ("48 3225 1540 / 32254428 / 3232 4395", ["(48) 3225-1540","32254428","3232 4395"], ["COMPLETO","INCOMPLETO","INCOMPLETO"]),
            ("51-3451-7729/937", ["(51) 3451-7729","937"], ["COMPLETO","INCOMPLETO"]),
            ("5134765101      51999774261   51991003252", ["(51) 3476-5101","(51) 99977-4261","(51) 99100-3252"], ["COMPLETO","COMPLETO","COMPLETO"]),
            ("51-3476-5101 - 51-9977-4261", ["(51) 3476-5101","51-9977-4261"], ["COMPLETO","INCOMPLETO"]),
            ("54991390676 - FILHA CLECI", ["(54) 99139-0676"], ["COMPLETO"]),
            ("(048) 233065800000", ["(048) 233065800000"], ["INCOMPLETO"]),
            ("0051 92859330", ["0051 92859330"], ["INCOMPLETO"]),
            ("0051 / 984585339", ["(51) 98458-5339"], ["COMPLETO"]),
            ("+51 997119087", ["+51 997119087"], ["INCOMPLETO"]),
            ("0000 00000000 / 984585339", ["0000 00000000","984585339"], ["INCOMPLETO","INCOMPLETO"]),
            ("/ ;", ["/ ;"], ["SEM_NUMERO"]),
            ("sem telefone", ["sem telefone"], ["SEM_NUMERO"]),
            ("5132336223 51-3227-2351", ["(51) 3233-6223","(51) 3227-2351"], ["COMPLETO","COMPLETO"]),
            ("51 / 33332364 / 999861341", ["(51) 3333-2364","999861341"], ["COMPLETO","INCOMPLETO"]),
            ("0055 32228896 / 0055 999951084", ["(55) 3222-8896","(55) 99995-1084"], ["COMPLETO","COMPLETO"]),
            ("FILHA / 51999861341 / NORA / 5132419416", ["(51) 99986-1341","(51) 3241-9416"], ["COMPLETO","COMPLETO"]),
            ("TEL 5132419416", ["(51) 3241-9416"], ["COMPLETO"]),
            ("54991390676 FILHA CLECI 51999861341", ["(54) 99139-0676","(51) 99986-1341"], ["COMPLETO","COMPLETO"]),
        };
        foreach (var test in tests)
        {
            var actual = Parse(test.Input);
            if (!actual.Select(p => p.Valor).SequenceEqual(test.Values) || !actual.Select(p => p.Situacao).SequenceEqual(test.Status))
                throw new Exception("Falha no parser V2: " + test.Input + " => " + string.Join(" | ", actual.Select(p => p.Valor + ":" + p.Situacao)));
            if (actual.Any(p => p.Valor.Length > 150)) throw new Exception("Valor excede a coluna.");
        }
        Console.WriteLine($"Parser V2: {tests.Length} cenarios aprovados.");
    }
}
