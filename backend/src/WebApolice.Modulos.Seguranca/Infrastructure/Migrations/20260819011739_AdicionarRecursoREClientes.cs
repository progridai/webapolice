using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguranca.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarRecursoREClientes : Migration
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
    SELECT id INTO v_modulo_id FROM seguranca.modulo WHERE codigo = 'CLIENTES';
    IF v_modulo_id IS NOT NULL THEN
        SELECT id INTO v_recurso_id FROM seguranca.recurso WHERE codigo = 'RE' AND modulo_id = v_modulo_id;
        IF v_recurso_id IS NULL THEN
            INSERT INTO seguranca.recurso (public_id, modulo_id, nome, codigo, ativo, habilitado)
            VALUES (gen_random_uuid(), v_modulo_id, 'Campo RE', 'RE', true, true);
        END IF;
    END IF;
END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
DECLARE
    v_modulo_id bigint;
BEGIN
    SELECT id INTO v_modulo_id FROM seguranca.modulo WHERE codigo = 'CLIENTES';
    IF v_modulo_id IS NOT NULL THEN
        DELETE FROM seguranca.recurso WHERE codigo = 'RE' AND modulo_id = v_modulo_id;
    END IF;
END $$;
            ");
        }
    }
}
