using System.Data;
using Npgsql;

namespace ClientesMigrationSimulator;

public static class TargetReader
{
    private static readonly Dictionary<string, string[]> Required = new()
    {
        ["core.pessoa"] = ["id", "public_id", "tipo_pessoa", "nome", "documento_principal", "documento_principal_limpo", "documento_valido", "data_nascimento", "sexo", "deleted_at"],
        ["core.pessoa_documento"] = ["pessoa_id", "tipo_documento", "numero", "orgao_emissor", "data_emissao"],
        ["core.pessoa_contato"] = ["pessoa_id", "tipo_contato", "valor", "valor_normalizado", "principal", "ativo"],
        ["core.pessoa_endereco"] = ["id", "pessoa_id", "cidade_id", "tipo_endereco", "cep", "logradouro", "numero", "complemento", "bairro", "principal", "ativo", "legado_situacao_endereco"],
        ["cadastro.cliente"] = ["id", "public_id", "pessoa_id", "status_id", "falecido", "data_obito", "data_cadastro_legado", "legado_id", "deleted_at"],
        ["cadastro.cliente_status"] = ["id", "codigo"],
        ["cadastro.cliente_vinculo"] = ["id", "cliente_id", "pessoa_id", "matricula", "lotacao_id", "banco_id", "agencia", "conta_corrente"],
        ["cadastro.cliente_dependente"] = ["id", "cliente_id", "pessoa_id", "tipo_relacao", "nome", "cpf", "data_nascimento", "rg", "orgao_rg", "data_emissao_rg"],
        ["legado.cliente_migration_map"] = ["legado_cliente_id", "pessoa_id", "cliente_id", "cliente_vinculo_id", "cpf_original", "cpf_limpo", "cpf_valido", "criterio_unificacao_pessoa", "criterio_criacao_vinculo"],
        ["core.cidade"] = ["id", "legado_id"], ["core.banco"] = ["id", "legado_id"],
        ["cadastro.lotacao"] = ["id", "legado_id"], ["convenio.siape_orgao"] = ["id", "legado_id"],
        ["convenio.siape_cliente"] = ["cliente_id", "pessoa_id", "cliente_vinculo_id", "siape", "orgao_id", "categoria", "setor", "instituto", "agencia", "funcao", "contrato", "digito_verificador", "instituidor"],
        ["convenio.corsan_cliente"] = ["cliente_id", "pessoa_id", "cliente_vinculo_id", "empresa", "rubrica", "grupo", "funcionario"]
    };

    public static async Task<TargetSnapshot> ReadAsync(string connectionString)
    {
        var snapshot = new TargetSnapshot { CapturedAt = DateTimeOffset.UtcNow };
        // Database-level enforcement, independent of a login's write privileges.
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        builder.Options = (builder.Options ?? "") + " -c default_transaction_read_only=on";
        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead);
        await using (var readOnly = new NpgsqlCommand("SET TRANSACTION READ ONLY", connection, transaction))
            await readOnly.ExecuteNonQueryAsync();

        async Task Query(string sql, Action<NpgsqlDataReader> consume)
        {
            await using var command = new NpgsqlCommand(sql, connection, transaction) { CommandTimeout = 180 };
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) consume(reader);
        }
        await Query("""
            SELECT table_schema || '.' || table_name, column_name, data_type,
                is_nullable = 'YES', column_default, character_maximum_length
            FROM information_schema.columns
            WHERE table_schema IN ('core', 'cadastro', 'convenio', 'legado')
            ORDER BY table_schema, table_name, ordinal_position
            """, r => snapshot.Columns.Add(new(r.GetString(0), r.GetString(1), r.GetString(2), r.GetBoolean(3),
                r.IsDBNull(4) ? null : r.GetString(4), r.IsDBNull(5) ? null : r.GetInt32(5))));
        await Query("""
            SELECT n.nspname || '.' || c.relname, k.conname, pg_get_constraintdef(k.oid)
            FROM pg_constraint k JOIN pg_class c ON c.oid = k.conrelid
            JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE n.nspname IN ('core', 'cadastro', 'convenio', 'legado')
            ORDER BY 1, 2
            """, r => snapshot.Constraints.Add(new(r.GetString(0), r.GetString(1), r.GetString(2))));
        await Query("""
            SELECT schemaname || '.' || tablename, indexname, indexdef FROM pg_indexes
            WHERE schemaname IN ('core', 'cadastro', 'convenio', 'legado') ORDER BY 1, 2
            """, r => snapshot.Indexes.Add(new(r.GetString(0), r.GetString(1), r.GetString(2))));
        foreach (var (table, columns) in Required)
            foreach (var column in columns)
                if (!snapshot.Columns.Any(c => c.Table == table && c.Name == column))
                    snapshot.SchemaIssues.Add($"Coluna ausente: {table}.{column}");

        bool Has(string table) => Required[table].All(name => snapshot.Columns.Any(c => c.Table == table && c.Name == name));
        if (Has("cadastro.cliente_status"))
        {
            var statuses = new List<long>();
            await Query("SELECT id::bigint FROM cadastro.cliente_status WHERE lower(btrim(codigo)) = 'ativo'", r => statuses.Add(r.GetInt64(0)));
            if (statuses.Count == 1) snapshot.ActiveStatusId = statuses[0];
            else snapshot.SchemaIssues.Add("Catálogo deve conter exatamente um status com código equivalente a ativo (sem diferença de caixa/espaços).");
        }
        foreach (var (catalog, table) in new Dictionary<string, string>
        {
            ["cidade"] = "core.cidade", ["banco"] = "core.banco", ["lotacao"] = "cadastro.lotacao", ["siape_orgao"] = "convenio.siape_orgao"
        })
        {
            if (!Has(table)) continue;
            // Table names are internal constants, never accepted from CLI/input.
            var duplicates = new HashSet<int>();
            await Query($"SELECT legado_id::integer, id::bigint FROM {table} WHERE legado_id IS NOT NULL", r =>
            {
                var legacy = r.GetInt32(0);
                if (!snapshot.Catalogs[catalog].TryAdd(legacy, r.GetInt64(1))) duplicates.Add(legacy);
            });
            foreach (var legacy in duplicates) snapshot.Catalogs[catalog].Remove(legacy);
            if (duplicates.Count > 0) snapshot.SchemaIssues.Add($"Catálogo {catalog}: {duplicates.Count} códigos legados ambíguos.");
        }
        if (Has("core.pessoa"))
            await Query("""
                SELECT id, documento_principal_limpo, nome, data_nascimento, sexo, deleted_at IS NOT NULL FROM core.pessoa
                """, r => snapshot.People.Add(new(r.GetInt64(0), r.IsDBNull(1) ? null : r.GetString(1), r.GetString(2),
                    r.IsDBNull(3) ? null : r.GetFieldValue<DateOnly>(3), r.IsDBNull(4) ? null : r.GetInt16(4), r.GetBoolean(5))));
        if (Has("cadastro.cliente"))
            await Query("SELECT id, pessoa_id, falecido, data_obito, deleted_at IS NOT NULL FROM cadastro.cliente", r =>
                snapshot.Clients.Add(new(r.GetInt64(0), r.GetInt64(1), r.GetBoolean(2),
                    r.IsDBNull(3) ? null : r.GetFieldValue<DateOnly>(3), r.GetBoolean(4))));
        if (Has("core.pessoa_endereco"))
            await Query("""
                SELECT id, pessoa_id, tipo_endereco, cep, logradouro, numero, complemento, bairro, cidade_id, principal, ativo
                FROM core.pessoa_endereco
                """, r => snapshot.Addresses.Add(new(r.GetInt64(0), r.GetInt64(1), String(r, 2), String(r, 3), String(r, 4),
                    String(r, 5), String(r, 6), String(r, 7), r.IsDBNull(8) ? null : r.GetInt64(8), r.GetBoolean(9), r.GetBoolean(10))));
        if (Has("legado.cliente_migration_map"))
            await Query("SELECT legado_cliente_id, pessoa_id, cliente_id, cliente_vinculo_id FROM legado.cliente_migration_map", r =>
            {
                var map = new TargetMap(r.GetInt32(0), r.GetInt64(1), r.GetInt64(2), r.IsDBNull(3) ? null : r.GetInt64(3));
                if (!snapshot.Maps.TryAdd(map.LegacyId, map)) snapshot.SchemaIssues.Add("Mapa possui cliente legado duplicado.");
            });
        snapshot.Verified = true;
        // No commit and no database writes. Release the repeatable-read snapshot.
        await transaction.RollbackAsync();
        return snapshot;
    }
    private static string? String(NpgsqlDataReader r, int index) => r.IsDBNull(index) ? null : r.GetString(index);
}
