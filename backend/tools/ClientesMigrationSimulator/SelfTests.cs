using System.Text.Json;

namespace ClientesMigrationSimulator;

// Focused executable tests, independent of databases and the main API's test fixtures.
public static class SelfTests
{
    public static int Run()
    {
        var assertions = 0;
        void Check(bool condition, string name)
        {
            if (!condition) throw new InvalidOperationException("Falhou: " + name);
            assertions++;
        }
        var today = new DateOnly(2026, 10, 13);
        SourceRow Row(int id, Dictionary<string, object?>? extra = null)
        {
            var values = new Dictionary<string, object?>
            {
                ["Cliente"] = id, ["Cli_Nome"] = "Pessoa Sintética", ["Cli_CPF_11"] = "52998224725",
                ["Cli_CPF"] = "11111111111", ["Cli_Data_Nascimento"] = "1980-01-01",
                ["Cli_Sexo"] = 0, ["Cli_Falecido"] = false, ["Cli_Endereco_Padrao"] = 1,
                ["Cli_Logradouro"] = "Rua A", ["Cli_LogradouroC"] = "Rua B"
            };
            if (extra is not null) foreach (var pair in extra) values[pair.Key] = pair.Value;
            return SourceRow.Parse(JsonSerializer.Serialize(values));
        }
        List<Plan> Analyze(params SourceRow[] rows) => Simulator.Analyze(rows.ToList(), new(), today);
        bool Has(Plan p, string code) => p.Issues.Any(i => i.Code == code);
        Check(Simulator.ValidCpf("52998224725"), "CPF com dígitos válidos");
        Check(Simulator.ValidCpf("01234567890"), "CPF mantém zero inicial");
        Check(!Simulator.ValidCpf("52998224724"), "CPF com dígito inválido");
        Check(!Simulator.ValidCpf("00000000000"), "CPF repetitivo inválido");
        Check(!Simulator.ValidCpf("529.982.247-25"), "não reconstruir CPF oficial");
        var p = Analyze(Row(1))[0];
        Check(p.Sex == 2 && p.StatusCode == "ativo", "sexo feminino convertido e status ativo");
        Check(!Has(p, "CPF_INVALIDO_SEM_UNIFICACAO"), "ignora CPF original para identificação");
        Check(p.Addresses.Single(a => a.Principal).Source == "SEM_C", "indicador 1");
        Check(p.Addresses.Single(a => a.Source == "SEM_C").Kind == "RESIDENCIAL"
            && p.Addresses.Single(a => a.Source == "C").Kind == "CORRESPONDENCIA"
            && !Has(p, "TIPO_ENDERECO_SEM_C_PENDENTE"), "tipos residencial e correspondência confirmados");
        p = Analyze(Row(1, new() { ["Cli_Endereco_Padrao"] = null }))[0];
        Check(p.Addresses.Single(a => a.Principal).Source == "SEM_C", "indicador nulo");
        p = Analyze(Row(1, new() { ["Cli_Endereco_Padrao"] = 0 }))[0];
        Check(p.Addresses.Single(a => a.Principal).Source == "C", "indicador 0 corresponde a correspondência");
        p = Analyze(Row(1, new() { ["Cli_LogradouroC"] = null, ["Cli_Endereco_Padrao"] = 0 }))[0];
        Check(p.Addresses.Count == 1 && p.Addresses[0].Principal && Has(p, "ENDERECO_PADRAO_FALLBACK"), "fallback selecionado vazio");
        p = Analyze(Row(1, new() { ["Cli_Logradouro"] = null, ["Cli_LogradouroC"] = null }))[0];
        Check(p.Addresses.Count == 0, "não cria endereço vazio");
        var merged = Analyze(Row(2), Row(1));
        Check(merged.Select(x => x.GroupKey).Distinct().Count() == 1, "duplicados compatíveis são candidatos");
        Check(merged.Sum(x => x.Addresses.Count(a => a.Principal)) == 1, "um principal entre endereços duplicados");
        var conflict = Analyze(Row(1), Row(2, new() { ["Cli_Nome"] = "Outra Pessoa" }));
        Check(conflict.All(x => Has(x, "CPF_DADOS_PRIORIZADOS_MAIOR_CLIENTE") && x.PersonalDataSourceLegacyId == 2
            && x.Classification != "BLOQUEADO") && conflict.Select(x => x.GroupKey).Distinct().Count() == 1, "conflito de nome resolvido pelo maior Cliente");
        var invalid = Analyze(Row(1, new() { ["Cli_CPF_11"] = "00000000000" }), Row(2, new() { ["Cli_CPF_11"] = "00000000000" }));
        Check(invalid.Select(x => x.GroupKey).Distinct().Count() == 2, "CPF inválido não unifica");
        p = Analyze(Row(1, new() { ["Cli_Nome"] = null }))[0];
        Check(p.Classification == "IGNORADO" && p.Addresses.Count == 0 && p.ChildCandidates.Count == 0, "nome ausente ignorado sem gerar filhos");
        p = Analyze(Row(1, new() { ["Cli_Falecido"] = true }))[0];
        Check(p.StatusCode == "ativo" && Has(p, "OBITO_AUSENTE_SUBSTITUIDO_1900")
            && p.Dates["Cli_Data_Obito"] == new DateOnly(1900, 1, 1), "falecido ativo com data padrão autorizada");
        p = Analyze(Row(1, new() { ["Cli_Data_Nascimento"] = "9988-04-27" }))[0];
        Check(Has(p, "DATA_INVALIDA_SUBSTITUIDA_1900") && p.Dates["Cli_Data_Nascimento"] == new DateOnly(1900, 1, 1)
            && p.Classification != "BLOQUEADO", "data futura substituída por padrão");
        p = Analyze(Row(1, new() { ["Grupo"] = 999, ["Subgrupo"] = 999, ["Cli_Status"] = "X", ["Cli_Sexo"] = 1 }))[0];
        Check(p.Sex == 1 && p.Issues.All(i => i.Field is not ("Grupo" or "Subgrupo" or "Cli_Status")), "ignorar campos definidos");
        var target = new TargetSnapshot { Verified = true, ActiveStatusId = 9 };
        target.People.Add(new(100, "52998224725", "Pessoa Sintética", new(1980, 1, 1), 2, false));
        target.Clients.Add(new(200, 100, false, null, false));
        target.Addresses.Add(new(300, 100, "CORRESPONDENCIA", null, "Existente", null, null, null, null, true, true));
        p = Simulator.Analyze([Row(1)], target, today)[0];
        Check(p.PersonId == 100 && p.ClientId == 200 && p.Addresses.All(a => !a.Principal), "reutilização candidata preserva principal existente");
        target.People.Add(new(101, "52998224725", "Pessoa Sintética", new(1980, 1, 1), 2, false));
        p = Simulator.Analyze([Row(1)], target, today)[0];
        Check(Has(p, "MULTIPLAS_PESSOAS_DESTINO"), "não escolhe primeira pessoa duplicada");
        target = new TargetSnapshot { Verified = true, ActiveStatusId = 9 };
        p = Simulator.Analyze([Row(1, new() { ["Cidade"] = 123 })], target, today)[0];
        Check(Has(p, "REFERENCIA_SEM_CORRESPONDENCIA"), "catálogo ausente bloqueia referência");
        target.SchemaIssues.Add("Coluna ausente");
        p = Simulator.Analyze([Row(1)], target, today)[0];
        Check(Has(p, "SCHEMA_DESTINO_INCOMPATIVEL"), "schema incompatível bloqueia");
        var addressesConflict = Analyze(Row(1), Row(2, new() { ["Cli_Logradouro"] = "Rua Diferente" }));
        Check(addressesConflict.All(x => Has(x, "ENDERECO_PADRAO_CONFLITO_RESOLVIDO_PRIMEIRO"))
            && addressesConflict.Sum(x => x.Addresses.Count(a => a.Principal)) == 1
            && addressesConflict[0].Addresses[0].Principal
            && addressesConflict.Sum(x => x.Addresses.Count) == 4
            && addressesConflict.All(x => x.Classification != "BLOQUEADO"), "preserva endereços distintos e primeiro principal resolve conflito");
        Check(conflict.All(x => x.Issues.Any(i => i.Code == "CPF_DIVERGENCIA_RESOLVIDA_MAIOR_CLIENTE" && i.Field == "Cli_Nome")), "informa campo pessoal resolvido");
        var explicitVsFallback = Analyze(Row(1, new() { ["Cli_Logradouro"] = null }), Row(2));
        Check(explicitVsFallback.Sum(x => x.Addresses.Count(a => a.Principal)) == 1
            && !explicitVsFallback.Any(x => Has(x, "ENDERECO_PADRAO_DIVERGENTE_ORIGEM")), "principal explícito prevalece sobre fallback");
        var allFallback = Analyze(Row(1, new() { ["Cli_Logradouro"] = null }), Row(2, new() { ["Cli_Logradouro"] = null, ["Cli_LogradouroC"] = "Outra Rua C" }));
        Check(allFallback[0].Addresses.Single().Principal && !allFallback[1].Addresses.Single().Principal
            && !allFallback.Any(x => Has(x, "ENDERECO_PADRAO_DIVERGENTE_ORIGEM")), "sem seleção disponível: primeiro endereço do grupo");
        target = new TargetSnapshot { Verified = true, ActiveStatusId = 9 };
        target.Maps.Add(1, new(1, 999, 888, null));
        p = Simulator.Analyze([Row(1)], target, today)[0];
        Check(Has(p, "MAPA_DESTINO_INCONSISTENTE"), "mapa órfão bloqueia");
        var repeat = Analyze(Row(1));
        Check(JsonSerializer.Serialize(repeat) == JsonSerializer.Serialize(Analyze(Row(1))), "plano determinístico na mesma data e entradas");
        var latest = Analyze(Row(9, new() { ["Cli_Sexo"] = 1, ["Cli_Falecido"] = true, ["Cli_Data_Obito"] = "2020-01-01" }), Row(1));
        Check(latest.All(x => x.PersonalDataSourceLegacyId == 9 && x.Sex == 1 && x.Deceased == true && !Has(x, "OBITO_SEM_FALECIDO")), "maior PK vence independente da ordem de entrada");
        latest = Analyze(Row(1, new() { ["Cli_Data_Nascimento"] = "9988-04-27" }), Row(9));
        Check(latest.All(x => !Has(x, "DATA_INVALIDA_OU_FUTURA")), "data pessoal antiga substituída não bloqueia");
        latest = Analyze(Row(1), Row(9, new() { ["Cli_Nome"] = null, ["Cli_Sexo"] = null }));
        Check(latest.Single(x => x.LegacyId == 9).Classification == "IGNORADO"
            && latest.Single(x => x.LegacyId == 1).PersonalDataSourceLegacyId == 1, "registro sem nome ignorado antes da seleção por maior Cliente");
        latest = Analyze(Row(1), Row(9, new() { ["Cli_Sexo"] = null }));
        Check(latest.All(x => x.Sex is null && x.PersonalDataSourceLegacyId == 9), "nulo pessoal do maior Cliente com nome prevalece");
        target = new TargetSnapshot { Verified = true, ActiveStatusId = 9 };
        target.People.Add(new(100, "52998224725", "Pessoa Sintética", new(1980, 1, 1), 2, false));
        latest = Simulator.Analyze([Row(1, new() { ["Cli_Nome"] = "Nome Antigo", ["Cli_Sexo"] = 1 }), Row(9)], target, today);
        Check(latest.All(x => !Has(x, "PESSOA_DIVERGENTE_DESTINO") && x.PersonId == 100), "comparação com destino usa apenas dados pessoais prioritários");
        p = Analyze(Row(1, new() { ["Cli_CPF_11"] = null }))[0];
        Check(p.Classification == "IGNORADO" && Has(p, "CPF_AUSENTE_IGNORADO") && p.Addresses.Count == 0, "sem CPF oficial ignorado mesmo com CPF original preenchido");
        p = Analyze(Row(1, new() { ["Cli_Data_Obito"] = "2020-01-01", ["Cli_Falecido"] = false }))[0];
        Check(p.Deceased == true && p.Dates["Cli_Data_Obito"] == new DateOnly(2020, 1, 1), "óbito existente marca falecido e mantém data");
        p = Analyze(Row(1, new() { ["Cli_Data_Obito"] = "2051-05-19", ["Cli_Falecido"] = false }))[0];
        Check(p.Deceased == true && p.Dates["Cli_Data_Obito"] == new DateOnly(1900, 1, 1)
            && p.Classification != "BLOQUEADO", "óbito futuro marca falecido com padrão sem bloqueio cronológico");
        p = Analyze(Row(1, new() { ["Cli_Data_Emissao_RG"] = "3197-10-13", ["Cli_Data_Nascimento_Conjuge"] = "9197-01-10" }))[0];
        Check(p.Dates["Cli_Data_Emissao_RG"] == new DateOnly(1900, 1, 1)
            && p.Dates["Cli_Data_Nascimento_Conjuge"] == new DateOnly(1900, 1, 1), "RG e nascimento de cônjuge inválidos usam padrão");
        latest = Analyze(Row(1), Row(9, new() { ["Cli_CPF_11"] = null }));
        Check(latest[0].PersonalDataSourceLegacyId == 1 && latest[1].Classification == "IGNORADO", "sem CPF não participa da prioridade");
        p = Analyze(Row(1, new() { ["Cli_Empresa"] = "Empresa", ["Cli_Rubrica"] = "123", ["Cidade_Lotacao"] = 1,
            ["Cli_Aposentado_Pensionista"] = 1, ["Cli_ant"] = 99, ["Cli_id_cliente_vind"] = "ABC", ["Cli_Empresa_Corsan"] = "Corsan" }))[0];
        Check(!Has(p, "CAMPO_SEM_DESTINO_CONFIRMADO") && p.ChildCandidates["convenio.corsan_cliente"] == 1,
            "seis campos desconsiderados sem afetar campos específicos Corsan");
        p = Analyze(Row(1, new() { ["Cli_Nome_Conjuge"] = "Cônjuge", ["Cli_RG_Conjuge"] = "RG123",
            ["Cli_Orgao_RG_Conjuge"] = "SSP", ["Cli_Data_Emissao_RG_Conjuge"] = "2000-01-01" }))[0];
        Check(p.SpouseSourceLegacyId == 1 && p.DependentRelationship == "CONJUGE"
            && p.ChildCandidates["cadastro.cliente_dependente"] == 1 && !Has(p, "CONJUGE_REGRA_PENDENTE"),
            "cônjuge tem destino e origem definidos incluindo documentos");
        target = new TargetSnapshot { Verified = true, ActiveStatusId = 1 };
        target.People.Add(new(100, "52998224725", "Cadastro Antigo", new(1980, 1, 1), 2, false));
        p = Simulator.Analyze([Row(9)], target, today, new Dictionary<int, long> { [9] = 100 })[0];
        Check(p.Classification != "BLOQUEADO" && Has(p, "PESSOA_PRIORIZADA_ORIGEM_AUTORIZADA"), "prioridade autorizada restrita à origem e pessoa especificadas");
        p = Simulator.Analyze([Row(9)], target, today, new Dictionary<int, long> { [9] = 101 })[0];
        Check(Has(p, "PESSOA_DIVERGENTE_DESTINO"), "não autoriza sobrescrever pessoa diferente");
        Console.WriteLine($"Testes sintéticos: {assertions} verificações passaram. Nenhuma conexão foi utilizada.");
        return 0;
    }
}
