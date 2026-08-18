using System;
using System.Threading.Tasks;
using Npgsql;

class Program
{
    static async Task Main(string[] args)
    {
        var prodConnString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(prodConnString))
        {
            Console.WriteLine("Erro: Variável de ambiente DB_CONNECTION_STRING não foi informada.");
            return;
        }

        var sql = @"
DO $$
DECLARE
    v_modulo_id bigint;
    v_recurso_id bigint;
BEGIN
    SELECT id INTO v_modulo_id FROM seguranca.modulo WHERE codigo = 'COOPERADOS';
    IF v_modulo_id IS NULL THEN
        INSERT INTO seguranca.modulo (public_id, nome, codigo, ativo, habilitado, ordem, created_at, updated_at) 
        VALUES (gen_random_uuid(), 'Cooperados', 'COOPERADOS', true, true, 15, now(), now())
        RETURNING id INTO v_modulo_id;
    END IF;

    SELECT id INTO v_recurso_id FROM seguranca.recurso WHERE codigo = 'COOPERADOS' AND modulo_id = v_modulo_id;
    IF v_recurso_id IS NULL THEN
        INSERT INTO seguranca.recurso (public_id, modulo_id, nome, codigo, ativo, created_at, updated_at)
        VALUES (gen_random_uuid(), v_modulo_id, 'Cooperados', 'COOPERADOS', true, now(), now())
        RETURNING id INTO v_recurso_id;
    END IF;

    INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, ativo, created_at, updated_at)
    SELECT gen_random_uuid(), v_recurso_id, 'Visualizar', 'cooperados.visualizar', true, now(), now()
    WHERE NOT EXISTS (SELECT 1 FROM seguranca.permissao WHERE codigo = 'cooperados.visualizar');

    INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, ativo, created_at, updated_at)
    SELECT gen_random_uuid(), v_recurso_id, 'Inserir', 'cooperados.inserir', true, now(), now()
    WHERE NOT EXISTS (SELECT 1 FROM seguranca.permissao WHERE codigo = 'cooperados.inserir');

    INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, ativo, created_at, updated_at)
    SELECT gen_random_uuid(), v_recurso_id, 'Alterar', 'cooperados.alterar', true, now(), now()
    WHERE NOT EXISTS (SELECT 1 FROM seguranca.permissao WHERE codigo = 'cooperados.alterar');

    INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, ativo, created_at, updated_at)
    SELECT gen_random_uuid(), v_recurso_id, 'Inativar', 'cooperados.inativar', true, now(), now()
    WHERE NOT EXISTS (SELECT 1 FROM seguranca.permissao WHERE codigo = 'cooperados.inativar');

    INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, ativo, created_at, updated_at)
    SELECT gen_random_uuid(), v_recurso_id, 'Reativar', 'cooperados.reativar', true, now(), now()
    WHERE NOT EXISTS (SELECT 1 FROM seguranca.permissao WHERE codigo = 'cooperados.reativar');

    INSERT INTO seguranca.perfil_permissao (perfil_id, permissao_id)
    SELECT p.id, perm.id
    FROM seguranca.perfil p
    CROSS JOIN seguranca.permissao perm
    WHERE p.nome = 'ADMINISTRADOR'
      AND perm.codigo IN (
        'cooperados.visualizar',
        'cooperados.inserir',
        'cooperados.alterar',
        'cooperados.inativar',
        'cooperados.reativar'
      )
      AND NOT EXISTS (
        SELECT 1 FROM seguranca.perfil_permissao pp 
        WHERE pp.perfil_id = p.id AND pp.permissao_id = perm.id
      );
END $$;

INSERT INTO ""__EFMigrationsHistory"" (migration_id, product_version)
SELECT '20260818125000_AdicionarModuloCooperados', '10.0.9'
WHERE NOT EXISTS (SELECT 1 FROM ""__EFMigrationsHistory"" WHERE migration_id = '20260818125000_AdicionarModuloCooperados');
";

        Console.WriteLine("Running against Prod DB...");
        await RunSqlAsync(prodConnString, sql);

        Console.WriteLine("Done!");
    }

    static async Task RunSqlAsync(string connString, string sql)
    {
        await using var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
        Console.WriteLine("Successfully ran SQL on " + conn.Database);
    }
}
