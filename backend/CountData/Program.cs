using System;
using Npgsql;

class Program
{
    static void Main()
    {
        string connString = "Host=painel.bravida.com.br;Port=5432;Database=webapolice_antigo;Username=bravito;Password=Bravida@2023!";
        using var conn = new NpgsqlConnection(connString);
        conn.Open();
        
        using var cmd = new NpgsqlCommand("SELECT tablename FROM pg_tables WHERE schemaname = 'public';", conn);
        using var reader = cmd.ExecuteReader();
        var tables = new System.Collections.Generic.List<string>();
        while(reader.Read()) tables.Add(reader.GetString(0));
        reader.Close();
        
        long totalRows = 0;
        foreach(var table in tables) {
            using var cmd2 = new NpgsqlCommand($"SELECT count(*) FROM "{table}"", conn);
            long count = (long)cmd2.ExecuteScalar();
            Console.WriteLine($"Table {table}: {count} rows");
            totalRows += count;
        }
        Console.WriteLine($"Total rows: {totalRows}");
    }
}
