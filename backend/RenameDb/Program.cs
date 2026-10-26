using System;
using Npgsql;

class Program
{
    static void Main()
    {
        string connString = Environment.GetEnvironmentVariable("WEBAPOLICE_ADMIN_CONNECTION")
            ?? throw new InvalidOperationException("Defina WEBAPOLICE_ADMIN_CONNECTION antes de executar a ferramenta.");
        if (string.IsNullOrWhiteSpace(connString)) throw new InvalidOperationException("WEBAPOLICE_ADMIN_CONNECTION não pode estar vazia.");
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
