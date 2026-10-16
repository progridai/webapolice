using System.Globalization;
using System.Text;

namespace ClientesMigrationSimulator;

public static class Simulator
{
    public static bool ValidCpf(string? cpf)
    {
        if (cpf is null || cpf.Length != 11 || cpf.Any(c => c < '0' || c > '9') || cpf.All(c => c == cpf[0])) return false;
        for (var length = 9; length <= 10; length++)
        {
            var sum = 0;
            for (var i = 0; i < length; i++) sum += (cpf[i] - '0') * (length + 1 - i);
            var check = sum * 10 % 11;
            if (check == 10) check = 0;
            if (check != cpf[length] - '0') return false;
        }
        return true;
    }

    public static string Normalize(string? value)
    {
        if (value is null) return "";
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var text = new string(decomposed.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());
        return string.Join(' ', text.ToUpperInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    public static List<Plan> Analyze(List<SourceRow> rows, TargetSnapshot target, DateOnly today,
        IReadOnlyDictionary<int, long>? authorizedOverrides = null)
    {
        var plans = rows.OrderBy(r => r.Id).ToDictionary(r => r.Id, r => AnalyzeRow(r, target, today));
        var peopleByCpf = target.People.Where(p => p.Cpf is not null).ToLookup(p => p.Cpf!);
        var clientsByPerson = target.Clients.ToLookup(c => c.PersonId);
        var addressesByPerson = target.Addresses.ToLookup(a => a.PersonId);
        var peopleById = target.People.ToDictionary(p => p.Id);
        var clientsById = target.Clients.ToDictionary(c => c.Id);

        foreach (var group in rows.Where(r => r.Text("Cli_Nome") is not null && r.Text("Cli_CPF_11") is not null)
            .GroupBy(r => ValidCpf(r.Text("Cli_CPF_11")) ? "CPF:" + r.Text("Cli_CPF_11") : "ID:" + r.Id))
        {
            var members = group.OrderBy(r => r.Id).ToList();
            var groupPlans = members.Select(r => plans[r.Id]).ToList();
            var preferred = members[^1];
            var preferredPlan = plans[preferred.Id];
            // Personal data comes from the greatest legacy PK, including null values.
            // Addresses and other child records retain their own source information.
            string[] personalFields = ["Cli_Nome", "Cli_Data_Nascimento", "Cli_Sexo", "Cli_Falecido", "Cli_Data_Obito"];
            var preferredIssues = preferredPlan.Issues.Where(i => personalFields.Contains(i.Field)).ToArray();
            var preferredDates = preferredPlan.Dates.Where(d => d.Key is "Cli_Data_Nascimento" or "Cli_Data_Obito").ToArray();
            foreach (var p in groupPlans)
            {
                p.PersonalDataSourceLegacyId = preferred.Id;
                p.Sex = preferredPlan.Sex;
                p.Deceased = preferredPlan.Deceased;
                foreach (var date in preferredDates) p.Dates[date.Key] = date.Value;
                p.Issues.RemoveAll(i => personalFields.Contains(i.Field));
                foreach (var issue in preferredIssues) p.Add(issue.Severity, issue.Code, issue.Field);
            }
            // Report keys never contain CPF. An unresolved group is isolated per source ID.
            var candidate = "origem:" + members[0].Id;
            var comparisons = new Dictionary<string, IEnumerable<string>>
            {
                ["Cli_Nome"] = members.Select(r => Normalize(r.Text("Cli_Nome"))),
                ["Cli_Data_Nascimento"] = members.Select(r => r.Date("Cli_Data_Nascimento")?.ToString("yyyy-MM-dd") ?? ""),
                ["Cli_Sexo"] = members.Select(r => r.Text("Cli_Sexo") ?? ""),
                ["Cli_Falecido"] = members.Select(r => r.Boolean("Cli_Falecido")?.ToString() ?? ""),
                ["Cli_Data_Obito"] = members.Select(r => r.Date("Cli_Data_Obito")?.ToString("yyyy-MM-dd") ?? "")
            };
            var divergentFields = comparisons.Where(pair => Conflicting(pair.Value)).Select(pair => pair.Key).ToList();
            if (divergentFields.Count > 0)
            {
                foreach (var p in groupPlans)
                {
                    p.Add("ADVERTENCIA", "CPF_DADOS_PRIORIZADOS_MAIOR_CLIENTE", "Cli_CPF_11");
                    foreach (var field in divergentFields) p.Add("ADVERTENCIA", "CPF_DIVERGENCIA_RESOLVIDA_MAIOR_CLIENTE", field);
                }
            }
            var cpf = members[0].Text("Cli_CPF_11");
            TargetPerson? person = null;
            TargetClient? client = null;
            if (target.Verified && ValidCpf(cpf))
            {
                var existing = peopleByCpf[cpf!].ToList();
                if (existing.Count > 1)
                    foreach (var p in groupPlans) p.Add("BLOQUEIO", "MULTIPLAS_PESSOAS_DESTINO", "Cli_CPF_11");
                else if (existing.Count == 1)
                {
                    person = existing[0];
                    var authorized = authorizedOverrides is not null && authorizedOverrides.TryGetValue(preferred.Id, out var authorizedPersonId) && authorizedPersonId == person.Id;
                    if (person.Deleted || Diverges(preferred, preferredPlan, person))
                        foreach (var p in groupPlans)
                            p.Add(authorized && !person.Deleted ? "ADVERTENCIA" : "BLOQUEIO",
                                person.Deleted ? "PESSOA_EXCLUIDA_DESTINO" : authorized ? "PESSOA_PRIORIZADA_ORIGEM_AUTORIZADA" : "PESSOA_DIVERGENTE_DESTINO", "Cli_CPF_11");
                    var clients = clientsByPerson[person.Id].ToList();
                    if (clients.Count > 1)
                        foreach (var p in groupPlans) p.Add("BLOQUEIO", "MULTIPLOS_CLIENTES_DESTINO", "cliente");
                    else if (clients.Count == 1)
                    {
                        client = clients[0];
                        if (client.Deleted
                            || (preferredPlan.Deceased is { } f && f != client.Deceased)
                            || (preferredPlan.Dates.GetValueOrDefault("Cli_Data_Obito") is { } d && client.Death is { } td && d != td))
                            foreach (var p in groupPlans) p.Add("BLOQUEIO", client.Deleted ? "CLIENTE_EXCLUIDO_DESTINO" : "FALECIMENTO_DIVERGENTE_DESTINO", "cliente");
                        // Existing clients are reviewed, never silently reactivated or overwritten.
                        foreach (var p in groupPlans) p.Add("ADVERTENCIA", "CLIENTE_EXISTENTE_REVISAR_STATUS_E_DADOS", "cliente");
                    }
                }
            }

            foreach (var p in groupPlans)
            {
                if (target.Maps.TryGetValue(p.LegacyId, out var map))
                {
                    peopleById.TryGetValue(map.PersonId, out var mappedPerson);
                    clientsById.TryGetValue(map.ClientId, out var mappedClient);
                    if (mappedPerson is null || mappedClient is null || mappedClient.PersonId != map.PersonId
                        || mappedPerson.Deleted || mappedClient.Deleted
                        || (ValidCpf(cpf) && mappedPerson.Cpf != cpf))
                        p.Add("BLOQUEIO", "MAPA_DESTINO_INCONSISTENTE", "Cliente");
                    else if (person is not null && person.Id != map.PersonId)
                        p.Add("BLOQUEIO", "MAPA_PESSOA_DIVERGENTE", "Cliente");
                    p.PersonId = map.PersonId;
                    p.ClientId = map.ClientId;
                    p.LinkId = map.LinkId;
                    p.Action = "JA_MAPEADO_REVISAR";
                    p.Add("ADVERTENCIA", "JA_MAPEADO_NAO_REIMPORTAR", "Cliente");
                }
                else
                {
                    p.PersonId = person?.Id;
                    p.ClientId = client?.Id;
                    p.Action = client is not null ? "REUTILIZAR_CLIENTE_CANDIDATO"
                        : person is not null ? "CRIAR_CLIENTE_PESSOA_EXISTENTE_CANDIDATO" : "CRIAR_CANDIDATO";
                }
                p.GroupKey = p.Classification == "BLOQUEADO" ? "origem:" + p.LegacyId
                    : p.PersonId is { } id ? "pessoa:" + id : candidate;
                if (members.Count > 1)
                    p.Add("ADVERTENCIA", "UNIFICACAO_CANDIDATA_REVISAR", "Cli_CPF_11");
            }

            // The same person can receive addresses from multiple compatible source records.
            // Select one primary across the whole group and preserve an existing primary.
            var resolvedPeople = groupPlans.Where(p => p.PersonId.HasValue).Select(p => p.PersonId!.Value).Distinct().ToList();
            var primaryPersonId = person?.Id ?? (resolvedPeople.Count == 1 ? resolvedPeople[0] : (long?)null);
            var principals = primaryPersonId is null ? [] : addressesByPerson[primaryPersonId.Value].Where(a => a.Principal && a.Active).ToList();
            if (principals.Count > 1)
                foreach (var p in groupPlans) p.Add("BLOQUEIO", "MULTIPLOS_ENDERECOS_PRINCIPAIS_DESTINO", "endereco");
            // Personal conflicts in the origin have already been resolved by greatest Cliente.
            {
                if (principals.Count > 0)
                {
                    foreach (var p in groupPlans)
                        for (var i = 0; i < p.Addresses.Count; i++) p.Addresses[i] = p.Addresses[i] with { Principal = false };
                }
                else
                {
                    var indicated = groupPlans.SelectMany(p => p.Addresses.Where(a => a.Principal).Select(a => (p, a))).ToList();
                    var explicitSelections = indicated.Where(x => !x.p.Issues.Any(i => i.Code == "ENDERECO_PADRAO_FALLBACK")).ToList();
                    if (explicitSelections.Count > 0)
                    {
                        foreach (var x in indicated.Except(explicitSelections))
                        {
                            var i = x.p.Addresses.IndexOf(x.a);
                            x.p.Addresses[i] = x.a with { Principal = false };
                        }
                        indicated = explicitSelections;
                    }
                    else if (indicated.Count > 1)
                    {
                        // No selected source address exists: the confirmed fallback is the first available.
                        foreach (var x in indicated.Skip(1))
                        {
                            var i = x.p.Addresses.IndexOf(x.a);
                            x.p.Addresses[i] = x.a with { Principal = false };
                        }
                        indicated = indicated.Take(1).ToList();
                    }
                    // Distinct selected addresses: identical addresses from duplicate rows are not a conflict.
                    var distinct = indicated.Select(x => AddressKey(members.First(r => r.Id == x.p.LegacyId), x.a.Source)).Distinct().Count();
                    if (distinct > 1)
                    {
                        // Confirmed rule: preserve every address candidate and select the first
                        // available address by ascending legacy ID, then SEM_C before C.
                        foreach (var p in groupPlans)
                        {
                            for (var i = 0; i < p.Addresses.Count; i++) p.Addresses[i] = p.Addresses[i] with { Principal = false };
                            p.Add("ADVERTENCIA", "ENDERECO_PADRAO_CONFLITO_RESOLVIDO_PRIMEIRO", "Cli_Endereco_Padrao");
                        }
                        var first = groupPlans.First(p => p.Addresses.Count > 0);
                        first.Addresses[0] = first.Addresses[0] with { Principal = true };
                    }
                    else if (indicated.Count > 1)
                        foreach (var x in indicated.Skip(1))
                        {
                            var i = x.p.Addresses.IndexOf(x.a);
                            x.p.Addresses[i] = x.a with { Principal = false };
                        }
                }
            }
        }
        foreach (var p in plans.Values.Where(p => p.Classification == "BLOQUEADO")) p.GroupKey = "origem:" + p.LegacyId;
        return plans.Values.OrderBy(p => p.LegacyId).ToList();
    }

    private static bool Conflicting(IEnumerable<string> values) => values.Where(v => v != "").Distinct().Take(2).Count() > 1;
    private static bool Diverges(SourceRow row, Plan selected, TargetPerson p) =>
        (row.Text("Cli_Nome") is { } n && Normalize(n) != Normalize(p.Name))
        || (selected.Dates.GetValueOrDefault("Cli_Data_Nascimento") is { } b && p.Birth is { } tb && b != tb)
        || (row.Number("Cli_Sexo") is { } s && p.Sex is { } ts && (s == 0 ? 2 : s) != ts);

    private static Plan AnalyzeRow(SourceRow r, TargetSnapshot target, DateOnly today)
    {
        var p = new Plan { LegacyId = r.Id, PersonalDataSourceLegacyId = r.Id, GroupKey = "origem:" + r.Id, Deceased = r.Boolean("Cli_Falecido") };
        if (r.Text("Cli_Nome") is null || r.Text("Cli_CPF_11") is null)
        {
            p.Action = r.Text("Cli_Nome") is null ? "IGNORAR_SEM_NOME" : "IGNORAR_SEM_CPF";
            if (r.Text("Cli_Nome") is null) p.Add("ADVERTENCIA", "NOME_AUSENTE_IGNORADO", "Cli_Nome");
            if (r.Text("Cli_CPF_11") is null) p.Add("ADVERTENCIA", "CPF_AUSENTE_IGNORADO", "Cli_CPF_11");
            return p;
        }
        if (!target.Verified) p.Add("ADVERTENCIA", "DESTINO_NAO_VERIFICADO", "destino");
        else if (target.SchemaIssues.Count > 0) p.Add("BLOQUEIO", "SCHEMA_DESTINO_INCOMPATIVEL", "destino");
        if (!ValidCpf(r.Text("Cli_CPF_11"))) p.Add("ADVERTENCIA", "CPF_INVALIDO_SEM_UNIFICACAO", "Cli_CPF_11");
        if (r.Text("Cli_Sexo") is not null && r.Number("Cli_Sexo") is not (0 or 1)) p.Add("BLOQUEIO", "SEXO_DESCONHECIDO", "Cli_Sexo");
        p.Sex = r.Number("Cli_Sexo") switch { 0 => 2, 1 => 1, _ => null };
        if (r.Text("Cli_Data_Obito") is not null && p.Deceased != true)
        {
            p.Deceased = true;
            p.Add("ADVERTENCIA", "FALECIDO_CORRIGIDO_POR_OBITO", "Cli_Falecido");
        }
        if (p.Deceased is null) p.Add("ADVERTENCIA", "FALECIDO_NULO_OU_INVALIDO_SEM_REGRA", "Cli_Falecido");
        if (r.Text("Cli_Data_Nascimento") is null) p.Add("ADVERTENCIA", "NASCIMENTO_AUSENTE", "Cli_Data_Nascimento");
        foreach (var field in new[] { "Cli_Data_Nascimento", "Cli_Data_Obito", "Cli_Data_Cadastro", "Cli_Data_Emissao_RG",
                     "Cli_Data_Nascimento_Conjuge", "Cli_Data_Emissao_RG_Conjuge" })
        {
            p.Dates[field] = r.Date(field);
            if (r.Text(field) is null) continue;
            var date = r.Date(field);
            if (date is null || date > today)
            {
                if (field == "Cli_Data_Cadastro") p.Add("BLOQUEIO", "DATA_INVALIDA_OU_FUTURA", field);
                else
                {
                    p.Dates[field] = new DateOnly(1900, 1, 1);
                    p.Add("ADVERTENCIA", "DATA_INVALIDA_SUBSTITUIDA_1900", field);
                }
            }
            else if (date < new DateOnly(1901, 1, 1)) p.Add("ADVERTENCIA", "DATA_ANTIGA_REVISAR", field);
        }
        if (p.Deceased == true && p.Dates.GetValueOrDefault("Cli_Data_Obito") is null)
        {
            p.Dates["Cli_Data_Obito"] = new DateOnly(1900, 1, 1);
            p.Add("ADVERTENCIA", "OBITO_AUSENTE_SUBSTITUIDO_1900", "Cli_Data_Obito");
        }
        if (p.Dates.GetValueOrDefault("Cli_Data_Obito") is { } death && death != new DateOnly(1900, 1, 1)
            && p.Dates.GetValueOrDefault("Cli_Data_Nascimento") is { } birth && death < birth)
        {
            p.Dates["Cli_Data_Obito"] = new DateOnly(1900, 1, 1);
            p.Add("ADVERTENCIA", "OBITO_ANTERIOR_NASCIMENTO_SUBSTITUIDO_1900", "Cli_Data_Obito");
        }

        // Confirmed exclusion: Cli_Empresa, Cli_Rubrica, Cidade_Lotacao,
        // Cli_Aposentado_Pensionista, Cli_ant and Cli_id_cliente_vind.
        if (r.HasContent("Cli_Nome_Conjuge", "Cli_CPF_Conjuge", "Cli_RG_Conjuge", "Cli_Data_Nascimento_Conjuge",
                "Cli_Data_Emissao_RG_Conjuge", "Cli_Orgao_RG_Conjuge"))
        {
            if (r.Text("Cli_Nome_Conjuge") is null) p.Add("ADVERTENCIA", "CONJUGE_SEM_NOME", "Cli_Nome_Conjuge");
            else
            {
                p.SpouseSourceLegacyId = r.Id;
                p.DependentRelationship = "CONJUGE";
            }
        }
        foreach (var field in new[] { "Cli_Telefone", "Cli_Telefone2", "Cli_Telefone3", "Cli_Telefone4" })
            if (r.Text(field) is { } phone && (phone.Contains(';') || phone.Contains('/') || phone.Contains(',') || phone.Count(char.IsDigit) > 13))
                p.Add("ADVERTENCIA", "TELEFONE_COMPOSTO_OU_SUSPEITO", field);
        if (r.Text("Cli_Email") is { } email && (!System.Net.Mail.MailAddress.TryCreate(email, out var parsed) || parsed.Address != email))
            p.Add("ADVERTENCIA", "EMAIL_INVALIDO_OU_COMPOSTO", "Cli_Email");
        if (r.Text("Cli_RG") is null && r.HasContent("Cli_Orgao_RG", "Cli_Data_Emissao_RG")) p.Add("ADVERTENCIA", "RG_DADOS_SEM_NUMERO", "Cli_RG");
        if (r.HasContent("Cli_Agencia", "Cli_Conta_Corrente") && r.Number("Banco") is null)
            p.Add("ADVERTENCIA", "CONTA_SEM_BANCO", "Banco");

        p.BankId = Catalog(r, p, target, "Banco", "banco");
        p.AllocationId = Catalog(r, p, target, "Lotacao", "lotacao");
        p.SiapeOrganizationId = Catalog(r, p, target, "Cli_Siape_Orgao", "siape_orgao");
        p.ChildCandidates["cadastro.cliente_vinculo"] = r.HasContent("Cli_Matricula", "Lotacao", "Banco", "Cli_Agencia", "Cli_Conta_Corrente") ? 1 : 0;
        p.ChildCandidates["core.pessoa_documento"] = r.Text("Cli_RG") is not null ? 1 : 0;
        p.ChildCandidates["core.pessoa_contato"] = new[] { "Cli_Telefone", "Cli_Telefone2", "Cli_Telefone3", "Cli_Telefone4", "Cli_Email" }.Count(f => r.Text(f) is not null);
        p.ChildCandidates["cadastro.cliente_dependente"] = p.SpouseSourceLegacyId.HasValue ? 1 : 0;
        p.ChildCandidates["convenio.siape_cliente"] = r.HasContent("Cli_Siape", "Cli_Siape_Orgao", "Cli_Siape_Categoria", "Cli_Siape_Setor", "Cli_Siape_Instituto", "Cli_Siape_Agencia", "Cli_Siape_Funcao", "Cli_Siape_Contrato", "Cli_Digito_Verificador_Siape", "Cli_Siape_Instituidor") ? 1 : 0;
        p.ChildCandidates["convenio.corsan_cliente"] = r.Boolean("Cli_Func_Corsan") == true || r.HasContent("Cli_Empresa_Corsan", "Cli_Rubrica_Corsan", "Cli_Grupo_Corsan") ? 1 : 0;
        AddAddress(r, p, target, "");
        AddAddress(r, p, target, "C");
        var selector = r.Number("Cli_Endereco_Padrao");
        if (r.Text("Cli_Endereco_Padrao") is not null && selector is not (0 or 1))
            p.Add("BLOQUEIO", "INDICADOR_ENDERECO_DESCONHECIDO", "Cli_Endereco_Padrao");
        var source = selector == 0 ? "C" : "SEM_C";
        var selected = p.Addresses.FindIndex(a => a.Source == source);
        if (selected < 0 && p.Addresses.Count > 0)
        {
            selected = 0;
            p.Add("ADVERTENCIA", "ENDERECO_PADRAO_FALLBACK", "Cli_Endereco_Padrao");
        }
        if (selected >= 0) p.Addresses[selected] = p.Addresses[selected] with { Principal = true };
        return p;
    }

    private static long? Catalog(SourceRow r, Plan p, TargetSnapshot target, string field, string catalog)
    {
        if (r.Text(field) is null) return null;
        if (r.Number(field) is not { } legacy)
        {
            p.Add("BLOQUEIO", "REFERENCIA_INVALIDA", field);
            return null;
        }
        if (!target.Verified) return null;
        if (target.Catalogs[catalog].TryGetValue(legacy, out var id)) return id;
        p.Add("BLOQUEIO", "REFERENCIA_SEM_CORRESPONDENCIA", field);
        return null;
    }

    private static void AddAddress(SourceRow r, Plan p, TargetSnapshot target, string suffix)
    {
        var cityField = "Cidade" + suffix;
        string[] fields = [cityField, "Cli_CEP" + suffix, "Cli_Logradouro" + suffix,
            "Cli_Numero" + suffix, "Cli_Complemento" + suffix, "Cli_Bairro" + suffix];
        if (!r.HasContent(fields)) return;
        var cityId = Catalog(r, p, target, cityField, "cidade");
        p.Addresses.Add(new AddressPlan(suffix == "" ? "SEM_C" : "C", suffix == "" ? "RESIDENCIAL" : "CORRESPONDENCIA", cityId, false));
    }

    private static string AddressKey(SourceRow r, string source)
    {
        var suffix = source == "C" ? "C" : "";
        return string.Join('|', new[] { "Cidade" + suffix, "Cli_CEP" + suffix, "Cli_Logradouro" + suffix,
            "Cli_Numero" + suffix, "Cli_Complemento" + suffix, "Cli_Bairro" + suffix }.Select(f => Normalize(r.Text(f))));
    }
}
