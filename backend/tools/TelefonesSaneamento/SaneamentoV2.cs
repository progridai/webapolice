using Npgsql;
using NpgsqlTypes;
using System.Text.Json;

static class SaneamentoV2
{
    public static async Task Execute(NpgsqlConnection connection, NpgsqlTransaction transaction, string sql)
    {
        await using var cmd = new NpgsqlCommand(sql, connection, transaction) { CommandTimeout = 900 };
        await using var reader = await cmd.ExecuteReaderAsync();
        do {
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++) row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                Console.WriteLine(JsonSerializer.Serialize(row));
            }
        } while (await reader.NextResultAsync());
    }
    public static async Task Run(NpgsqlConnection connection, NpgsqlTransaction transaction, string[] args)
    {
        TelefoneParserV2.Test();
        var elapsed = System.Diagnostics.Stopwatch.StartNew();
        var folder = Path.Combine(Directory.GetCurrentDirectory(), "backend/tools/TelefonesSaneamento");
        await Execute(connection, transaction, File.ReadAllText(Path.Combine(folder, "v2-plano.sql")));
        var sources = new List<(long Id, string Valor, string Tipo)>();
        await using (var cmd = new NpgsqlCommand("""
            SELECT c.id,c.valor,c.tipo_contato
            FROM core.pessoa_contato c JOIN legado.telefone_saneamento_v1 v ON v.contato_origem_id=c.id
            WHERE v.situacao='REVISAO' AND c.ativo AND c.tipo_contato IN ('TELEFONE','CELULAR')
            AND NOT EXISTS(SELECT 1 FROM legado.telefone_saneamento_v2 s WHERE s.contato_origem_id=c.id)
            ORDER BY c.id
            """, connection, transaction) { CommandTimeout = 300 })
        await using (var reader = await cmd.ExecuteReaderAsync())
            while (await reader.ReadAsync()) sources.Add((reader.GetInt64(0), reader.GetString(1),reader.GetString(2)));
        Console.WriteLine(JsonSerializer.Serialize(new { origens_lidas_v2 = sources.Count, segundos = elapsed.Elapsed.TotalSeconds }));
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await Execute(connection, transaction, "CREATE TEMP TABLE telefone_v2_carga(id bigint PRIMARY KEY,propostas jsonb NOT NULL) ON COMMIT DROP;");
        await using (var copy = await connection.BeginBinaryImportAsync("COPY telefone_v2_carga(id,propostas) FROM STDIN (FORMAT BINARY)"))
        {
            foreach (var source in sources)
            {
                var parts = TelefoneParserV2.Parse(source.Valor, source.Tipo);
                if (parts.Any(p => p.Valor.Length > 150 || p.Normalizado.Length > 150)) throw new Exception("Trecho excede os limites; nenhuma truncagem permitida.");
                await copy.StartRowAsync();
                await copy.WriteAsync(source.Id, NpgsqlDbType.Bigint);
                await copy.WriteAsync(JsonSerializer.Serialize(parts, options), NpgsqlDbType.Jsonb);
            }
            await copy.CompleteAsync();
        }
        // The contact table has been locked since the start; capture the exact before-image server-side.
        await Execute(connection, transaction, """
            INSERT INTO legado.telefone_saneamento_v2(contato_origem_id,pessoa_id,original,propostas)
            SELECT c.id,c.pessoa_id,to_jsonb(c),s.propostas FROM telefone_v2_carga s JOIN core.pessoa_contato c ON c.id=s.id;
            """);
        Console.WriteLine(JsonSerializer.Serialize(new { origens_planejadas_v2 = sources.Count, segundos = elapsed.Elapsed.TotalSeconds }));
        foreach (var script in args.Where(a => !a.StartsWith("--")))
            await Execute(connection, transaction, File.ReadAllText(script));
    }
}
