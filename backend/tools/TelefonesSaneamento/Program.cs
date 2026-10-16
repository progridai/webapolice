using Npgsql;
using System.Text.Json;

if (args.Contains("--self-test")) { TelefoneParserV2.Test(); return; }
var root = Directory.GetCurrentDirectory();
var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__POSTGRESQLTESTES");
if (string.IsNullOrWhiteSpace(connectionString))
{
    var line = File.ReadLines(Path.Combine(root, "backend/.env.local")).First(l => l.StartsWith("ConnectionStrings__PostgreSqlTestes=", StringComparison.OrdinalIgnoreCase));
    connectionString = line[(line.IndexOf('=') + 1)..].Trim().Trim('"', '\'');
}
var builder = new NpgsqlConnectionStringBuilder(connectionString);
if (builder.Database != "webapolice_teste") throw new Exception("Destino deve ser webapolice_teste.");
await using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();
await using var transaction = await connection.BeginTransactionAsync();
var apply = args.Contains("--apply");
var validate = args.Contains("--validate");
if (apply && validate) throw new Exception("Escolha apply ou validate.");
await using (var setup = new NpgsqlCommand((apply || validate ? "" : "SET TRANSACTION READ ONLY; ") + "SET LOCAL statement_timeout='300s'; SET LOCAL lock_timeout='15s';", connection, transaction)) await setup.ExecuteNonQueryAsync();
if (args.Contains("--v2"))
{
    if (!apply && !validate) throw new Exception("V2 exige --validate ou --apply.");
    await SaneamentoV2.Run(connection, transaction, args);
    if (apply) await transaction.CommitAsync(); else await transaction.RollbackAsync();
    Console.WriteLine(apply ? "COMMIT" : "ROLLBACK");
    return;
}
await using var command = new NpgsqlCommand(string.Join("\n", args.Where(a => !a.StartsWith("--")).Select(File.ReadAllText)), connection, transaction) { CommandTimeout = 900 };
await using var reader = await command.ExecuteReaderAsync();
do {
    while (await reader.ReadAsync())
    {
        var row = new Dictionary<string, object?>();
        for (var i = 0; i < reader.FieldCount; i++) row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
        Console.WriteLine(JsonSerializer.Serialize(row));
    }
} while (await reader.NextResultAsync());
await reader.DisposeAsync();
if (apply) await transaction.CommitAsync(); else await transaction.RollbackAsync();
Console.WriteLine(apply ? "COMMIT" : "ROLLBACK");
