using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ClientesMigrationSimulator;
using Npgsql;
using NpgsqlTypes;

try
{
    string? source = null, output = null;
    var apply = false;
    var validate = false;
    for (var i = 0; i < args.Length; i++)
        switch (args[i])
        {
            case "--source-dir" when i + 1 < args.Length: source = Path.GetFullPath(args[++i]); break;
            case "--output" when i + 1 < args.Length: output = Path.GetFullPath(args[++i]); break;
            case "--apply": apply = true; break;
            case "--validate-transaction": validate = true; break;
            default: throw new ArgumentException("Use --source-dir PASTA --output PASTA_NOVA [--apply | --validate-transaction].");
        }
    if (source is null || output is null || Directory.Exists(output) || File.Exists(output) || (apply && validate))
        throw new ArgumentException("Informe origem e pasta de saída nova; escolha somente um modo de escrita.");
    var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(source, "manifesto-carga.json")));
    if (manifest.RootElement.GetProperty("targetDatabase").GetString() != "webapolice_teste") throw new ArgumentException("Destino não autorizado.");
    var expected = manifest.RootElement.GetProperty("sourceSha256").GetString()!;
    if (!manifest.RootElement.TryGetProperty("authorizedPersonOverrides", out var authorized)
        || !authorized.TryGetProperty("435698", out var person) || person.GetInt64() != 1)
        throw new ArgumentException("Manifesto precisa da prioridade autorizada: 435698 → pessoa 1.");
    var overrides = new Dictionary<int, long> { [435698] = 1 };
    using var stream = new FileStream(Path.Combine(source, "clientes.jsonl"), FileMode.Open, FileAccess.Read, FileShare.Read);
    var sourceHash = Convert.ToHexString(SHA256.HashData(stream));
    if (!sourceHash.Equals(expected, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("A origem mudou após a análise.");
    stream.Position = 0;
    var rows = new List<SourceRow>();
    var ids = new HashSet<int>();
    using var reader = new StreamReader(stream, Encoding.UTF8, true, leaveOpen: true);
    while (reader.ReadLine() is { } line)
    {
        if (string.IsNullOrWhiteSpace(line)) continue;
        var row = SourceRow.Parse(line);
        if (row.Id <= 0 || !ids.Add(row.Id) || SourceRow.RequiredFields.Any(f => !row.Has(f))) throw new ArgumentException("Origem incompleta ou ID duplicado.");
        rows.Add(row);
    }
    Console.WriteLine($"Origem validada: {rows.Count} cadastros.");
    var connectionString = Environment.GetEnvironmentVariable("WEBAPOLICE_MIGRATION_TARGET") ?? throw new ArgumentException("Defina WEBAPOLICE_MIGRATION_TARGET.");
    if (new NpgsqlConnectionStringBuilder(connectionString).Database != "webapolice_teste") throw new ArgumentException("Somente webapolice_teste é autorizado.");
    var target = await TargetReader.ReadAsync(connectionString);
    var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, "America/Sao_Paulo").DateTime);
    var plans = Simulator.Analyze(rows, target, today, overrides);
    Directory.CreateDirectory(output);
    await Reports.WriteAsync(output, plans, rows, target, sourceHash, today);
    if (plans.Any(p => p.Classification == "BLOQUEADO")) throw new ArgumentException("Há bloqueios; consulte os relatórios. Nenhuma carga executada.");
    var byId = plans.ToDictionary(p => p.LegacyId);
    var eligible = rows.Where(r => byId[r.Id].Classification != "IGNORADO").ToList();
    if (eligible.Count == 0) throw new ArgumentException("Nenhum registro elegível.");
    var missingSpouses = rows.Where(r => byId[r.Id].Issues.Any(i => i.Code == "CONJUGE_SEM_NOME")).Select(r => r.Id).ToList();
    await File.WriteAllTextAsync(Path.Combine(output, "conjuges-nao-importados.json"), JsonSerializer.Serialize(new { Reason = "SEM_NOME", LegacyIds = missingSpouses }));
    // No truncation: reject values that would exceed destination limits.
    foreach (var r in eligible)
        foreach (var (field, limit) in new[] { ("Cli_CEP",20), ("Cli_CEPC",20), ("Cli_Nome",150), ("Cli_Orgao_RG",50) })
            if (r.Text(field)?.Length > limit) throw new ArgumentException($"Campo {field} excede limite no cliente {r.Id}.");
    if (!apply && !validate)
    {
        Console.WriteLine($"Plano preparado: {eligible.Count} registros elegíveis; {plans.Count - eligible.Count} ignorados. Nenhum banco alterado.");
        return 0;
    }
    // A complete retry needs no staging/COPY and must not overwrite later edits.
    var mappedInSnapshot = eligible.Count(r => target.Maps.ContainsKey(r.Id));
    if (mappedInSnapshot > 0)
    {
        if (mappedInSnapshot != eligible.Count) throw new ArgumentException("Mapas parciais encontrados; revisar antes de retomar.");
        await using var checkConnection = new NpgsqlConnection(connectionString);
        await checkConnection.OpenAsync();
        await using var checkCommand = new NpgsqlCommand("SELECT count(*) FROM legado.cliente_migration_map WHERE observacao=@batch",checkConnection);
        checkCommand.Parameters.AddWithValue("batch","MIGRACAO:"+sourceHash);
        var batchCount = Convert.ToInt64(await checkCommand.ExecuteScalarAsync());
        if (batchCount != eligible.Count) throw new ArgumentException("A correspondência existente pertence a outra carga ou está incompleta.");
        await File.WriteAllTextAsync(Path.Combine(output,"resultado-carga.json"),JsonSerializer.Serialize(new { AlreadyImported=true, Count=batchCount, SourceSha256=sourceHash }));
        Console.WriteLine("Extração já importada integralmente; nenhuma duplicação ou atualização realizada.");
        return 0;
    }
    await using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable);
    async Task<int> Execute(string sql)
    {
        await using var cmd = new NpgsqlCommand(sql, connection, transaction) { CommandTimeout = 900 };
        return await cmd.ExecuteNonQueryAsync();
    }
    await Execute("SET LOCAL lock_timeout='15s'; SELECT pg_advisory_xact_lock(20261015,1);");
    await Execute("LOCK TABLE core.pessoa,cadastro.cliente,cadastro.cliente_vinculo,core.pessoa_documento,core.pessoa_contato,core.pessoa_endereco,cadastro.cliente_dependente,convenio.siape_cliente,convenio.corsan_cliente,legado.cliente_migration_map IN SHARE ROW EXCLUSIVE MODE;");
    // Preserve a recoverable before-image for the explicitly authorized existing record.
    await using (var backup = new NpgsqlCommand("SELECT jsonb_build_object('pessoa',to_jsonb(p),'cliente',to_jsonb(c))::text FROM core.pessoa p LEFT JOIN cadastro.cliente c ON c.pessoa_id=p.id WHERE p.id=1", connection, transaction))
    {
        var value = await backup.ExecuteScalarAsync();
        await File.WriteAllTextAsync(Path.Combine(output, "cadastro-existente-antes.json"), value?.ToString() ?? "null", Encoding.UTF8);
    }
    await Execute("CREATE TEMP TABLE migration_source(legacy_id integer PRIMARY KEY,group_key text NOT NULL,preferred integer NOT NULL,person_id bigint,client_id bigint,sex smallint,deceased boolean NOT NULL,data jsonb NOT NULL) ON COMMIT DROP;");
    await using (var copy = await connection.BeginBinaryImportAsync("COPY migration_source(legacy_id,group_key,preferred,person_id,client_id,sex,deceased,data) FROM STDIN(FORMAT BINARY)"))
    {
        foreach (var r in eligible)
        {
            var p = byId[r.Id];
            var data = SourceRow.RequiredFields.Where(f => r.Text(f) is not null).ToDictionary(f => f, f => (object?)r.Text(f));
            foreach (var date in p.Dates) data[date.Key] = date.Value?.ToString("yyyy-MM-dd");
            data["addresses"] = p.Addresses.Select(a =>
            {
                var suffix = a.Source == "C" ? "C" : "";
                return new { type=a.Kind, cityId=a.CityId, cep=r.Text("Cli_CEP"+suffix), street=r.Text("Cli_Logradouro"+suffix),
                    number=r.Text("Cli_Numero"+suffix), complement=r.Text("Cli_Complemento"+suffix), neighborhood=r.Text("Cli_Bairro"+suffix),
                    principal=a.Principal, state=r.Number("Cli_Situacao_Endereco"+suffix) };
            }).ToArray();
            data["contacts"] = new[] { "Cli_Telefone", "Cli_Telefone2", "Cli_Telefone3", "Cli_Telefone4", "Cli_Email" }
                .Where(f => r.Text(f) is not null).Select((f, index) => new
                {
                    type=f=="Cli_Email" ? "EMAIL" : "TELEFONE", value=r.Text(f), normalized=Simulator.Normalize(r.Text(f)), order=index
                }).ToArray();
            data["linkKey"] = p.ChildCandidates.GetValueOrDefault("cadastro.cliente_vinculo") == 1
                ? new object?[] { Simulator.Normalize(r.Text("Cli_Matricula")), p.AllocationId, p.BankId, r.Text("Cli_Agencia"), r.Text("Cli_Conta_Corrente") } : null;
            data["lotacaoId"] = p.AllocationId;
            data["bancoId"] = p.BankId;
            data["orgaoId"] = p.SiapeOrganizationId;
            data["hasSiape"] = p.ChildCandidates.GetValueOrDefault("convenio.siape_cliente") == 1;
            data["hasCorsan"] = p.ChildCandidates.GetValueOrDefault("convenio.corsan_cliente") == 1;
            data["matriculaNormalizada"] = Simulator.Normalize(r.Text("Cli_Matricula"));
            data["nomeNormalizado"] = r.Text("Cli_Nome")?.ToUpperInvariant();
            data["cpfValido"] = Simulator.ValidCpf(r.Text("Cli_CPF_11"));
            var spouseCpf = new string((r.Text("Cli_CPF_Conjuge") ?? "").Where(char.IsDigit).ToArray());
            data["spouseKey"] = r.Text("Cli_Nome_Conjuge") is null ? null : Simulator.ValidCpf(spouseCpf)
                ? "CPF:"+spouseCpf : "NOME:"+Simulator.Normalize(r.Text("Cli_Nome_Conjuge"))+"|"+p.Dates.GetValueOrDefault("Cli_Data_Nascimento_Conjuge")?.ToString("yyyy-MM-dd");
            await copy.StartRowAsync();
            await copy.WriteAsync(r.Id, NpgsqlDbType.Integer);
            await copy.WriteAsync(p.GroupKey, NpgsqlDbType.Text);
            await copy.WriteAsync(p.PersonalDataSourceLegacyId, NpgsqlDbType.Integer);
            if (p.PersonId is { } pid) await copy.WriteAsync(pid,NpgsqlDbType.Bigint); else await copy.WriteNullAsync();
            if (p.ClientId is { } cid) await copy.WriteAsync(cid,NpgsqlDbType.Bigint); else await copy.WriteNullAsync();
            if (p.Sex is { } sex) await copy.WriteAsync(sex,NpgsqlDbType.Smallint); else await copy.WriteNullAsync();
            await copy.WriteAsync(p.Deceased ?? false,NpgsqlDbType.Boolean);
            await copy.WriteAsync(JsonSerializer.Serialize(data),NpgsqlDbType.Jsonb);
        }
        await copy.CompleteAsync();
    }
    Console.WriteLine("Dados preparados no banco em transação; conferindo histórico de migração.");
    await using var mapped = new NpgsqlCommand("SELECT count(*),count(*) FILTER(WHERE m.observacao=@batch) FROM legado.cliente_migration_map m JOIN migration_source s ON s.legacy_id=m.legado_cliente_id",connection,transaction);
    mapped.Parameters.AddWithValue("batch", "MIGRACAO:"+sourceHash);
    long mappedCount, sameBatch;
    await using (var rd=await mapped.ExecuteReaderAsync()) { await rd.ReadAsync(); mappedCount=rd.GetInt64(0); sameBatch=rd.GetInt64(1); }
    if (mappedCount != 0)
    {
        if (mappedCount != eligible.Count || sameBatch != eligible.Count) throw new ArgumentException("Mapas de outra carga/parciais encontrados; operação interrompida.");
        await transaction.RollbackAsync();
        await File.WriteAllTextAsync(Path.Combine(output,"resultado-carga.json"),JsonSerializer.Serialize(new { AlreadyImported=true, Count=mappedCount, SourceSha256=sourceHash }));
        Console.WriteLine("Extração já importada integralmente; nenhuma duplicação ou atualização realizada.");
        return 0;
    }
    var script = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory,"load.sql"));
    await Execute(script);
    await using (var mapCmd=new NpgsqlCommand("UPDATE legado.cliente_migration_map m SET observacao=@batch FROM migration_source s WHERE m.legado_cliente_id=s.legacy_id",connection,transaction))
    { mapCmd.Parameters.AddWithValue("batch","MIGRACAO:"+sourceHash); await mapCmd.ExecuteNonQueryAsync(); }
    await using var verify = new NpgsqlCommand("""
        SELECT jsonb_build_object(
          'sourceRows',(SELECT count(*) FROM migration_source),
          'groups',(SELECT count(*) FROM migration_groups),
          'mapped',(SELECT count(*) FROM legado.cliente_migration_map m JOIN migration_source s ON m.legado_cliente_id=s.legacy_id),
          'invalidMaps',(SELECT count(*) FROM migration_source s LEFT JOIN legado.cliente_migration_map m ON m.legado_cliente_id=s.legacy_id
            LEFT JOIN core.pessoa p ON p.id=m.pessoa_id LEFT JOIN cadastro.cliente c ON c.id=m.cliente_id
            WHERE m.id IS NULL OR p.id IS NULL OR c.id IS NULL OR c.pessoa_id<>p.id OR c.status_id<>(SELECT id FROM cadastro.cliente_status WHERE lower(codigo)='ativo')),
          'links',(SELECT count(*) FROM migration_links),
          'addresses',(SELECT count(*) FROM core.pessoa_endereco a JOIN migration_groups g ON a.pessoa_id=g.person_id),
          'contacts',(SELECT count(*) FROM core.pessoa_contato a JOIN migration_groups g ON a.pessoa_id=g.person_id),
          'documents',(SELECT count(*) FROM core.pessoa_documento a JOIN migration_groups g ON a.pessoa_id=g.person_id),
          'dependents',(SELECT count(*) FROM cadastro.cliente_dependente a JOIN migration_groups g ON a.cliente_id=g.client_id),
          'badPrimaryAddress',(SELECT count(*) FROM (SELECT g.person_id,count(*) FILTER(WHERE a.principal) AS n
             FROM migration_groups g JOIN core.pessoa_endereco a ON a.pessoa_id=g.person_id AND a.ativo
             GROUP BY g.person_id HAVING count(*) FILTER(WHERE a.principal)<>1) q))::text
        """,connection,transaction) { CommandTimeout=900 };
    var verification=(string)(await verify.ExecuteScalarAsync())!;
    using var json=JsonDocument.Parse(verification);
    if (json.RootElement.GetProperty("mapped").GetInt64()!=eligible.Count || json.RootElement.GetProperty("invalidMaps").GetInt64()!=0
        || json.RootElement.GetProperty("badPrimaryAddress").GetInt64()!=0) throw new ArgumentException("Conciliação falhou; transação revertida.");
    await File.WriteAllTextAsync(Path.Combine(output,"conciliacao-transacao.json"),verification,Encoding.UTF8);
    if (apply) await transaction.CommitAsync(); else await transaction.RollbackAsync();
    await File.WriteAllTextAsync(Path.Combine(output,"resultado-carga.json"),JsonSerializer.Serialize(new { Applied=apply, RolledBack=!apply, SourceSha256=sourceHash, Eligible=eligible.Count, Ignored=rows.Count-eligible.Count, MissingSpouses=missingSpouses.Count, Verification=JsonSerializer.Deserialize<JsonElement>(verification) }),Encoding.UTF8);
    Console.WriteLine(apply ? "Carga de clientes confirmada e conciliada em webapolice_teste." : "Validação transacional concluída com ROLLBACK; nenhum dado de cliente mantido.");
    return 0;
}
catch(ArgumentException ex) { Console.Error.WriteLine(ex.Message); return 1; }
catch(PostgresException ex) { Console.Error.WriteLine($"PostgreSQL recusou a operação. SQLSTATE={ex.SqlState}; tabela={ex.TableName}; coluna={ex.ColumnName}; constraint={ex.ConstraintName}. A transação não confirmada é revertida."); return 1; }
catch(Exception ex) { Console.Error.WriteLine($"Operação interrompida ({ex.GetType().Name}); conferir relatórios e configuração sem divulgar credenciais."); return 1; }
