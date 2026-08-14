using System;
using Npgsql;

class Program {
    static void Main() {
        string connStr = "Host=painel.bravida.com.br;Port=5432;Database=webapolice_antigo;Username=bravito;Password=Bravida@2023!";
        using var conn = new NpgsqlConnection(connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand("SELECT table_schema, table_name FROM information_schema.tables WHERE table_schema = 'core'", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) {
            Console.WriteLine($"{reader.GetString(0)}.{reader.GetString(1)}");
        }
    }
}
