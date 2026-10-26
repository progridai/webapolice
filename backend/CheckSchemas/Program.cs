using System;
using Npgsql;

class Program {
    static void Main() {
        string connStr = Environment.GetEnvironmentVariable("WEBAPOLICE_SOURCE_CONNECTION")
            ?? throw new InvalidOperationException("Defina WEBAPOLICE_SOURCE_CONNECTION antes de executar a ferramenta.");
        if (string.IsNullOrWhiteSpace(connStr)) throw new InvalidOperationException("WEBAPOLICE_SOURCE_CONNECTION não pode estar vazia.");
        using var conn = new NpgsqlConnection(connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand("SELECT table_schema, table_name FROM information_schema.tables WHERE table_schema = 'core'", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) {
            Console.WriteLine($"{reader.GetString(0)}.{reader.GetString(1)}");
        }
    }
}
