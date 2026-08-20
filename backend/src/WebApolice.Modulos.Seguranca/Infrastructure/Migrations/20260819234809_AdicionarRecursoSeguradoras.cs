using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguranca.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarRecursoSeguradoras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
DECLARE
    v_modulo_id bigint;
    v_recurso_id bigint;
BEGIN
    INSERT INTO seguranca.modulo (public_id, nome, codigo, ativo, habilitado, ordem)
    VALUES ('81820615-0000-0000-0000-000000000001', 'Cadastro', 'CADASTRO', true, true, 15)
    ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome, habilitado = EXCLUDED.habilitado
    RETURNING id INTO v_modulo_id;

    IF v_modulo_id IS NOT NULL THEN
        INSERT INTO seguranca.recurso (public_id, modulo_id, nome, codigo, ativo, habilitado)
        VALUES ('81820611-0000-0000-0000-000000000015', v_modulo_id, 'Seguradoras', 'SEGURADORAS', true, true)
        ON CONFLICT (modulo_id, codigo) DO UPDATE SET nome = EXCLUDED.nome, habilitado = EXCLUDED.habilitado
        RETURNING id INTO v_recurso_id;

        INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, ativo)
        VALUES 
            (gen_random_uuid(), v_recurso_id, 'Visualizar', 'seguradoras.visualizar', true),
            (gen_random_uuid(), v_recurso_id, 'Inserir', 'seguradoras.inserir', true),
            (gen_random_uuid(), v_recurso_id, 'Alterar', 'seguradoras.alterar', true),
            (gen_random_uuid(), v_recurso_id, 'Inativar', 'seguradoras.inativar', true),
            (gen_random_uuid(), v_recurso_id, 'Reativar', 'seguradoras.reativar', true)
        ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome;

        -- Atribui ao perfil ADMINISTRATIVO
        INSERT INTO seguranca.perfil_permissao (perfil_id, permissao_id)
        SELECT p.id, perm.id
        FROM seguranca.perfil p
        CROSS JOIN seguranca.permissao perm
        WHERE p.codigo = 'ADMINISTRATIVO'
          AND perm.codigo IN ('seguradoras.visualizar', 'seguradoras.inserir', 'seguradoras.alterar', 'seguradoras.inativar', 'seguradoras.reativar')
        ON CONFLICT (perfil_id, permissao_id) DO NOTHING;
    END IF;
END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM seguranca.permissao WHERE recurso_id IN (SELECT id FROM seguranca.recurso WHERE codigo = 'SEGURADORAS');
DELETE FROM seguranca.recurso WHERE codigo = 'SEGURADORAS';
            ");
        }
    }
}
