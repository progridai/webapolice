using System;
using Npgsql;

class Program
{
    static void Main()
    {
        string connString = Environment.GetEnvironmentVariable("WEBAPOLICE_SOURCE_CONNECTION")
            ?? throw new InvalidOperationException("Defina WEBAPOLICE_SOURCE_CONNECTION antes de executar a ferramenta.");
        if (string.IsNullOrWhiteSpace(connString)) throw new InvalidOperationException("WEBAPOLICE_SOURCE_CONNECTION não pode estar vazia.");
        using var conn = new NpgsqlConnection(connString);
        conn.Open();
        
        using var cmd = new NpgsqlCommand("SELECT tablename FROM pg_tables WHERE schemaname = 'public';", conn);
        using var reader = cmd.ExecuteReader();
        var tables = new System.Collections.Generic.List<string>();
        while(reader.Read()) tables.Add(reader.GetString(0));
        reader.Close();
        
        long totalRows = 0;
        using var sqlBuilder = new NpgsqlCommandBuilder();
        foreach(var table in tables) {
            using var cmd2 = new NpgsqlCommand("SELECT count(*) FROM " + sqlBuilder.QuoteIdentifier(table), conn);
            long count = cmd2.ExecuteScalar() is long result
                ? result
                : throw new InvalidOperationException("A consulta de contagem não retornou um inteiro.");
            Console.WriteLine($"Table {table}: {count} rows");
            totalRows += count;
        }
        Console.WriteLine($"Total rows: {totalRows}");
    }
}
