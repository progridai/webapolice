using System;
using Npgsql;

namespace SqlRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSql")
            ?? throw new InvalidOperationException("Defina ConnectionStrings__PostgreSql antes de executar a ferramenta.");
        if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidOperationException("ConnectionStrings__PostgreSql não pode estar vazia.");

            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            using var command = new NpgsqlCommand(@"
                ALTER TABLE seguro.apolice_vida 
                ADD COLUMN IF NOT EXISTS apolice_subestipulante_modulo_id bigint;

                ALTER TABLE seguro.apolice_vida
                DROP CONSTRAINT IF EXISTS fk_av_apolice_subestipulante_modulo;

                ALTER TABLE seguro.apolice_vida
                ADD CONSTRAINT fk_av_apolice_subestipulante_modulo 
                FOREIGN KEY (apolice_subestipulante_modulo_id) 
                REFERENCES seguro.apolice_subestipulante_modulo (id) ON DELETE SET NULL;
            ", connection);
            
            command.ExecuteNonQuery();

            Console.WriteLine("Coluna apolice_subestipulante_modulo_id adicionada com sucesso!");
        }
    }
}
