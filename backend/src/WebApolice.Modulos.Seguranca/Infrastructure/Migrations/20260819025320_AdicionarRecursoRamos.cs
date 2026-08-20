using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguranca.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarRecursoRamos : Migration
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
    SELECT id INTO v_modulo_id FROM seguranca.modulo WHERE codigo = 'APOLICES';
    
    IF v_modulo_id IS NOT NULL THEN
        INSERT INTO seguranca.recurso (public_id, modulo_id, nome, codigo, ativo, habilitado)
        VALUES ('81820611-0000-0000-0000-000000000014', v_modulo_id, 'Ramos', 'RAMOS', true, true)
        ON CONFLICT (modulo_id, codigo) DO UPDATE SET nome = EXCLUDED.nome, habilitado = EXCLUDED.habilitado
        RETURNING id INTO v_recurso_id;

        INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, ativo)
        VALUES 
            (gen_random_uuid(), v_recurso_id, 'Visualizar', 'ramos.visualizar', true),
            (gen_random_uuid(), v_recurso_id, 'Inserir', 'ramos.inserir', true),
            (gen_random_uuid(), v_recurso_id, 'Alterar', 'ramos.alterar', true),
            (gen_random_uuid(), v_recurso_id, 'Inativar', 'ramos.inativar', true),
            (gen_random_uuid(), v_recurso_id, 'Reativar', 'ramos.reativar', true)
        ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome;
    END IF;
END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM seguranca.permissao WHERE recurso_id IN (SELECT id FROM seguranca.recurso WHERE codigo = 'RAMOS');
DELETE FROM seguranca.recurso WHERE codigo = 'RAMOS';
            ");
        }
    }
}
