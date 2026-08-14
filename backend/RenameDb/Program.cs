using System;
using Npgsql;

class Program
{
    static void Main()
    {
        string connString = "Host=painel.bravida.com.br;Port=5432;Database=postgres;Username=bravito;Password=Bravida@2023!";
        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        using (var cmd = new NpgsqlCommand("SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = 'webapolice_teste' AND pid <> pg_backend_pid();", conn))
        {
            cmd.ExecuteNonQuery();
        }

        using (var cmd = new NpgsqlCommand("ALTER DATABASE webapolice_teste RENAME TO webapolice_antigo;", conn))
        {
            cmd.ExecuteNonQuery();
        }
        Console.WriteLine("Database renamed.");
    }
}
