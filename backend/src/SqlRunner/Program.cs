using Npgsql;
using System;

class Program
{
    static void Main()
    {
        var connString = "Host=painel.bravida.com.br;Port=5432;Database=webapolice_teste;Username=bravito;Password=Bravida@2023!;";
        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        var sql = @"
            CREATE TABLE IF NOT EXISTS seguro.apolice_subestipulante_modulo (
                id bigint GENERATED ALWAYS AS IDENTITY,
                apolice_subestipulante_id bigint NOT NULL,
                modulo_id bigint NOT NULL,
                data_inicio date,
                data_fim date,
                ativo boolean NOT NULL DEFAULT TRUE,
                created_at timestamp with time zone NOT NULL DEFAULT (now()),
                updated_at timestamp with time zone NOT NULL DEFAULT (now()),
                deleted_at timestamp with time zone,
                CONSTRAINT pk_apolice_subestipulante_modulo PRIMARY KEY (id),
                CONSTRAINT fk_apolice_subestipulante_modulo_apolice_subestipulante_apolic FOREIGN KEY (apolice_subestipulante_id) REFERENCES seguro.apolice_subestipulante (id) ON DELETE RESTRICT
            );

            CREATE INDEX IF NOT EXISTS ix_apolice_subestipulante_modulo_apolice_subestipulante_id ON seguro.apolice_subestipulante_modulo (apolice_subestipulante_id);
            CREATE UNIQUE INDEX IF NOT EXISTS ix_apolice_subestipulante_modulo_apolice_subestipulante_id_mod ON seguro.apolice_subestipulante_modulo (apolice_subestipulante_id, modulo_id) WHERE deleted_at IS NULL;
        ";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.ExecuteNonQuery();
        Console.WriteLine("Table created successfully!");
    }
}
