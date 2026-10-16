using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ClientesMigrationSimulator;
using Npgsql;
using NpgsqlTypes;

if (args.Length == 0 || args.Contains("--help"))
{
    Console.WriteLine("Carga de catálogos em webapolice_teste. --source-dir PASTA --output ARQUIVO_NOVO [--apply].\nConexão: WEBAPOLICE_MIGRATION_TARGET. Sem --apply, apenas prepara o plano local.");
    return 0;
}
try
{
    string? source = null, output = null;
    var apply = false;
    for (var i = 0; i < args.Length; i++)
        switch (args[i])
        {
            case "--source-dir" when i + 1 < args.Length: source = Path.GetFullPath(args[++i]); break;
            case "--output" when i + 1 < args.Length: output = Path.GetFullPath(args[++i]); break;
            case "--apply": apply = true; break;
            default: throw new ArgumentException("Opção inválida; consulte --help.");
        }
    if (source is null || output is null || File.Exists(output)) throw new ArgumentException("Informe origem e um arquivo de saída novo.");
    using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(source, "manifesto-carga.json")));
    if (manifest.RootElement.GetProperty("targetDatabase").GetString() != "webapolice_teste") throw new ArgumentException("Destino do manifesto não autorizado.");
    var expectedHashes = manifest.RootElement.GetProperty("catalogs").EnumerateArray()
        .ToDictionary(x => x.GetProperty("file").GetString()!, x => x.GetProperty("sha256").GetString()!);
    expectedHashes["clientes.jsonl"] = manifest.RootElement.GetProperty("sourceSha256").GetString()!;
    var references = new HashSet<int>();
    var rows = new List<CatalogRow>();
    foreach (var file in new[] { "clientes.jsonl", "cidades.jsonl", "bancos.jsonl", "lotacoes.jsonl", "orgaos-siape.jsonl" })
    {
        using var stream = new FileStream(Path.Combine(source, file), FileMode.Open, FileAccess.Read, FileShare.Read);
        if (!Convert.ToHexString(SHA256.HashData(stream)).Equals(expectedHashes[file], StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("A extração mudou após a análise: " + file);
        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8, true, leaveOpen: true);
        var ids = new HashSet<int>();
        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var r = SourceRow.Parse(line);
            if (file == "clientes.jsonl")
            {
                if (r.Text("Cli_Nome") is not null && r.Text("Cli_CPF_11") is not null && r.Number("Lotacao") is { } allocation) references.Add(allocation);
                continue;
            }
            var idField = file switch { "cidades.jsonl" => "Cidade", "bancos.jsonl" => "Banco", "lotacoes.jsonl" => "Lotacao", _ => "Orgao" };
            var id = r.Number(idField) ?? throw new ArgumentException("ID inválido no catálogo " + file);
            if (!ids.Add(id)) throw new ArgumentException("ID duplicado no catálogo " + file);
            if (file == "lotacoes.jsonl" && !references.Contains(id)) continue;
            var name = r.Text(file switch { "cidades.jsonl" => "Cid_Nome", "bancos.jsonl" => "Ban_Nome", "lotacoes.jsonl" => "Lot_Nome", _ => "Nome" });
            if (file == "lotacoes.jsonl" && id == 3714 && name is null) name = "Lotação não informada";
            if (name is null || name.Length > (file == "orgaos-siape.jsonl" ? 150 : 100)) throw new ArgumentException("Nome ausente/excedido em " + file + ", ID " + id);
            var code = r.Text(file switch { "bancos.jsonl" => "Ban_Codigo", "lotacoes.jsonl" => "Lot_Codigo", _ => "Codigo" });
            var uf = r.Text("Cid_UF")?.ToUpperInvariant();
            if (uf?.Length > 2 || code?.Length > (file == "bancos.jsonl" ? 20 : 50)) throw new ArgumentException("UF/código excede limite em " + file + ", ID " + id);
            rows.Add(new(file, id, name, code, uf, r.Number("Lot_Cidade"), r.Text("Ban_Observacoes")));
        }
    }
    var cityIds = rows.Where(r => r.Kind == "cidades.jsonl").Select(r => r.Id).ToHashSet();
    var importedAllocations = rows.Where(r => r.Kind == "lotacoes.jsonl").Select(r => r.Id).ToHashSet();
    if (!references.IsSubsetOf(importedAllocations)) throw new ArgumentException("Existem lotações referenciadas ausentes na extração.");
    var unknownAllocationCities = rows.Where(r => r.City.HasValue && !cityIds.Contains(r.City.Value))
        .Select(r => new { LegacyAllocationId = r.Id, LegacyCityId = r.City }).ToList();
    var counts = rows.GroupBy(r => r.Kind).ToDictionary(g => g.Key, g => g.Count());
    var inserted = new Dictionary<string, int>();
    if (apply)
    {
        var value = Environment.GetEnvironmentVariable("WEBAPOLICE_MIGRATION_TARGET") ?? throw new ArgumentException("Defina WEBAPOLICE_MIGRATION_TARGET.");
        var builder = new NpgsqlConnectionStringBuilder(value);
        if (builder.Database != "webapolice_teste") throw new ArgumentException("Escrita permitida somente em webapolice_teste.");
        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable);
        async Task<int> Execute(string sql)
        {
            await using var command = new NpgsqlCommand(sql, connection, transaction) { CommandTimeout = 180 };
            return await command.ExecuteNonQueryAsync();
        }
        async Task<long> Scalar(string sql)
        {
            await using var command = new NpgsqlCommand(sql, connection, transaction) { CommandTimeout = 180 };
            return Convert.ToInt64(await command.ExecuteScalarAsync());
        }
        if (await Scalar("SELECT CASE WHEN current_database() = 'webapolice_teste' THEN 1 ELSE 0 END") != 1) throw new ArgumentException("Banco incorreto.");
        await Execute("SET LOCAL lock_timeout = '10s'; SELECT pg_advisory_xact_lock(20261015, 1);");
        await Execute("""
            CREATE TEMP TABLE migration_catalogos (
                kind text NOT NULL, legado_id integer NOT NULL, nome text NOT NULL,
                codigo text, uf text, cidade_legado_id integer, observacao text,
                PRIMARY KEY (kind,legado_id)) ON COMMIT DROP;
            """);
        await using (var writer = await connection.BeginBinaryImportAsync("COPY migration_catalogos(kind,legado_id,nome,codigo,uf,cidade_legado_id,observacao) FROM STDIN (FORMAT BINARY)"))
        {
            foreach (var row in rows)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(row.Kind, NpgsqlDbType.Text);
                await writer.WriteAsync(row.Id, NpgsqlDbType.Integer);
                await writer.WriteAsync(row.Name, NpgsqlDbType.Text);
                foreach (var text in new[] { row.Code, row.Uf })
                    if (text is null) await writer.WriteNullAsync(); else await writer.WriteAsync(text, NpgsqlDbType.Text);
                if (row.City is { } city) await writer.WriteAsync(city, NpgsqlDbType.Integer); else await writer.WriteNullAsync();
                if (row.Observation is null) await writer.WriteNullAsync(); else await writer.WriteAsync(row.Observation, NpgsqlDbType.Text);
            }
            await writer.CompleteAsync();
        }
        // Existing records must match; these statements never overwrite destination data.
        inserted["core.cidade"] = await Execute("""
            INSERT INTO core.cidade(nome,nome_normalizado,uf,estado_id,legado_id)
            SELECT s.nome,upper(s.nome),s.uf,e.id,s.legado_id FROM migration_catalogos s
            LEFT JOIN core.estado e ON e.uf=s.uf WHERE s.kind='cidades.jsonl'
            ON CONFLICT (legado_id) WHERE legado_id IS NOT NULL DO NOTHING;
            """);
        inserted["core.banco"] = await Execute("""
            INSERT INTO core.banco(nome,codigo,observacao,legado_id)
            SELECT nome,codigo,observacao,legado_id FROM migration_catalogos WHERE kind='bancos.jsonl'
            ON CONFLICT (legado_id) WHERE legado_id IS NOT NULL DO NOTHING;
            """);
        inserted["cadastro.lotacao"] = await Execute("""
            INSERT INTO cadastro.lotacao(nome,codigo,cidade_id,legado_id)
            SELECT s.nome,s.codigo,c.id,s.legado_id FROM migration_catalogos s
            LEFT JOIN core.cidade c ON c.legado_id=s.cidade_legado_id WHERE s.kind='lotacoes.jsonl'
            ON CONFLICT (legado_id) WHERE legado_id IS NOT NULL DO NOTHING;
            """);
        inserted["convenio.siape_orgao"] = await Execute("""
            INSERT INTO convenio.siape_orgao(nome,codigo,legado_id)
            SELECT nome,codigo,legado_id FROM migration_catalogos WHERE kind='orgaos-siape.jsonl'
            ON CONFLICT (legado_id) WHERE legado_id IS NOT NULL DO NOTHING;
            """);
        var mismatch = await Scalar("""
            SELECT count(*) FROM (
              SELECT s.legado_id FROM migration_catalogos s LEFT JOIN core.cidade d ON d.legado_id=s.legado_id
              WHERE s.kind='cidades.jsonl' AND (d.id IS NULL OR d.nome IS DISTINCT FROM s.nome OR d.uf IS DISTINCT FROM s.uf)
              UNION ALL
              SELECT s.legado_id FROM migration_catalogos s LEFT JOIN core.banco d ON d.legado_id=s.legado_id
              WHERE s.kind='bancos.jsonl' AND (d.id IS NULL OR d.nome IS DISTINCT FROM s.nome OR d.codigo IS DISTINCT FROM s.codigo OR d.observacao IS DISTINCT FROM s.observacao)
              UNION ALL
              SELECT s.legado_id FROM migration_catalogos s LEFT JOIN cadastro.lotacao d ON d.legado_id=s.legado_id
              LEFT JOIN core.cidade c ON c.legado_id=s.cidade_legado_id
              WHERE s.kind='lotacoes.jsonl' AND (d.id IS NULL OR d.nome IS DISTINCT FROM s.nome OR d.codigo IS DISTINCT FROM s.codigo OR d.cidade_id IS DISTINCT FROM c.id)
              UNION ALL
              SELECT s.legado_id FROM migration_catalogos s LEFT JOIN convenio.siape_orgao d ON d.legado_id=s.legado_id
              WHERE s.kind='orgaos-siape.jsonl' AND (d.id IS NULL OR d.nome IS DISTINCT FROM s.nome OR d.codigo IS DISTINCT FROM s.codigo)
            ) conflicts;
            """);
        if (mismatch != 0) throw new ArgumentException("Catálogos existentes divergem da origem. Transação revertida sem sobrescrita.");
        await transaction.CommitAsync();
    }
    var report = new { Target = "webapolice_teste", Applied = apply, Counts = counts, Inserted = inserted,
        Allocation3714 = "Lotação não informada", UnknownAllocationCities = unknownAllocationCities,
        SourceHashes = expectedHashes, GeneratedAt = DateTimeOffset.UtcNow };
    await File.WriteAllTextAsync(output, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }), Encoding.UTF8);
    Console.WriteLine(apply ? "Catálogos aplicados e conciliados em uma transação. Nenhum cliente foi importado." : "Plano de catálogos preparado; nenhum banco alterado.");
    foreach (var pair in counts) Console.WriteLine($"{pair.Key}: {pair.Value}");
    return 0;
}
catch (ArgumentException ex) { Console.Error.WriteLine(ex.Message); return 1; }
catch (Exception ex) { Console.Error.WriteLine($"Operação interrompida ({ex.GetType().Name}). Confira configuração, extração e schema; não registre credenciais em logs."); return 1; }

record CatalogRow(string Kind, int Id, string Name, string? Code, string? Uf, int? City, string? Observation);
