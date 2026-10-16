using System.Globalization;
using System.Text.Json;

namespace ClientesMigrationSimulator;

public sealed record Issue(string Severity, string Code, string Field);
public sealed record AddressPlan(string Source, string Kind, long? CityId, bool Principal);
public sealed class Plan
{
    public int LegacyId { get; init; }
    public int PersonalDataSourceLegacyId { get; set; }
    public string GroupKey { get; set; } = "";
    public string StatusCode { get; init; } = "ativo";
    public short? Sex { get; set; }
    public bool? Deceased { get; set; }
    public Dictionary<string, DateOnly?> Dates { get; } = [];
    public long? PersonId { get; set; }
    public long? ClientId { get; set; }
    public long? LinkId { get; set; }
    public long? BankId { get; set; }
    public long? AllocationId { get; set; }
    public long? SiapeOrganizationId { get; set; }
    public int? SpouseSourceLegacyId { get; set; }
    public string? DependentRelationship { get; set; }
    public Dictionary<string, int> ChildCandidates { get; } = [];
    public string Action { get; set; } = "CRIAR_CANDIDATO";
    public List<AddressPlan> Addresses { get; } = [];
    public List<Issue> Issues { get; } = [];
    public string Classification => Action.StartsWith("IGNORAR_", StringComparison.Ordinal) ? "IGNORADO"
        : Issues.Any(i => i.Severity == "BLOQUEIO") ? "BLOQUEADO"
        : Issues.Count > 0 ? "COM_ADVERTENCIAS" : "PRONTO_PARA_REVISAO";
    public void Add(string severity, string code, string field)
    {
        var issue = new Issue(severity, code, field);
        if (!Issues.Contains(issue)) Issues.Add(issue);
    }
}

// Keeps all source fields for analysis; the original file is never modified.
public sealed class SourceRow(Dictionary<string, string?> values)
{
    public static readonly string[] RequiredFields = [
        "Cliente", "Cidade", "Grupo", "Subgrupo", "Lotacao", "Banco", "Cli_Nome", "Cli_Matricula",
        "Cli_Data_Nascimento", "Cli_Sexo", "Cli_Falecido", "Cli_CPF", "Cli_CPF_11", "Cli_Telefone",
        "Cli_RG", "Cli_Orgao_RG", "Cli_Data_Emissao_RG", "Cli_CEP", "Cli_Logradouro", "Cli_Numero",
        "Cli_Complemento", "Cli_Bairro", "Cli_Email", "Cli_Agencia", "Cli_Conta_Corrente",
        "Cli_Nome_Conjuge", "Cli_Data_Nascimento_Conjuge", "Cli_CPF_Conjuge", "Cli_RG_Conjuge",
        "Cli_Data_Emissao_RG_Conjuge", "Cli_Orgao_RG_Conjuge", "Cli_Empresa_Corsan", "Cli_Rubrica_Corsan",
        "Cli_Grupo_Corsan", "Cli_Func_Corsan", "Cli_Observacoes", "Cli_ant", "Cli_Status", "Cidade_Lotacao",
        "Cli_Siape", "Cli_Siape_Orgao", "Cli_Siape_Categoria", "Cli_Siape_Setor", "Cli_Siape_Instituto",
        "Cli_Siape_Agencia", "Cli_Siape_Funcao", "Cli_Siape_Contrato", "Cli_Situacao_Endereco",
        "Cli_Digito_Verificador_Siape", "Cli_Siape_Instituidor", "Cli_Data_Obito", "Cli_CEPC",
        "Cli_LogradouroC", "Cli_NumeroC", "Cli_ComplementoC", "Cli_BairroC", "CidadeC",
        "Cli_Situacao_EnderecoC", "Cli_Endereco_Padrao", "Cli_Aposentado_Pensionista",
        "Cli_Data_Cadastro", "Cli_Telefone2", "Cli_Telefone3", "Cli_Telefone4", "Cli_Empresa",
        "Cli_Rubrica", "Cli_id_cliente_vind"
    ];
    public int Id => int.Parse(Text("Cliente")!, CultureInfo.InvariantCulture);
    public string? Text(string field)
    {
        if (!values.TryGetValue(field, out var s)) return null;
        return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
    public bool Has(string field) => values.ContainsKey(field);
    public bool HasContent(params string[] fields) => fields.Any(f => Text(f) is not null);
    public int? Number(string field) => int.TryParse(Text(field), out var n) ? n : null;
    public DateOnly? Date(string field) => DateTime.TryParse(Text(field), CultureInfo.InvariantCulture,
        DateTimeStyles.None, out var dt) ? DateOnly.FromDateTime(dt) : null;
    public bool? Boolean(string field) => Text(field) switch
    {
        "true" or "1" => true, "false" or "0" => false, _ => null
    };
    public static SourceRow Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object) throw new FormatException("Objeto JSON ausente.");
        var fields = new Dictionary<string, string?>();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            var v = property.Value;
            if (v.ValueKind is JsonValueKind.Object or JsonValueKind.Array) throw new FormatException("Campo não escalar.");
            var value = v.ValueKind == JsonValueKind.Null ? null : v.ValueKind == JsonValueKind.String ? v.GetString() : v.GetRawText();
            if (!fields.TryAdd(property.Name, value)) throw new FormatException("Campo duplicado.");
        }
        return new SourceRow(fields);
    }
}

public sealed record TargetPerson(long Id, string? Cpf, string Name, DateOnly? Birth, short? Sex, bool Deleted);
public sealed record TargetClient(long Id, long PersonId, bool Deceased, DateOnly? Death, bool Deleted);
public sealed record TargetAddress(long Id, long PersonId, string? Kind, string? Cep, string? Street,
    string? Number, string? Complement, string? Neighborhood, long? CityId, bool Principal, bool Active);
public sealed record TargetMap(int LegacyId, long PersonId, long ClientId, long? LinkId);
public sealed record ColumnInfo(string Table, string Name, string Type, bool Nullable, string? Default, int? MaxLength);
public sealed record ConstraintInfo(string Table, string Name, string Definition);
public sealed record IndexInfo(string Table, string Name, string Definition);
public sealed class TargetSnapshot
{
    public bool Verified { get; set; }
    public DateTimeOffset? CapturedAt { get; set; }
    public long? ActiveStatusId { get; set; }
    public List<ColumnInfo> Columns { get; } = [];
    public List<ConstraintInfo> Constraints { get; } = [];
    public List<IndexInfo> Indexes { get; } = [];
    public Dictionary<string, Dictionary<int, long>> Catalogs { get; } = new()
    {
        ["cidade"] = [], ["banco"] = [], ["lotacao"] = [], ["siape_orgao"] = []
    };
    public List<TargetPerson> People { get; } = [];
    public List<TargetClient> Clients { get; } = [];
    public List<TargetAddress> Addresses { get; } = [];
    public Dictionary<int, TargetMap> Maps { get; } = [];
    public List<string> SchemaIssues { get; } = [];
}
