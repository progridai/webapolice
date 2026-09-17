using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguranca.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarPermissoesSubgruposApolice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
DECLARE
    v_recurso_id bigint;
BEGIN
    -- Subgrupos pertencem ao mesmo recurso APOLICES
    SELECT id INTO v_recurso_id FROM seguranca.recurso WHERE codigo = 'APOLICES';

    IF v_recurso_id IS NOT NULL THEN
        INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, ativo)
        VALUES 
            (gen_random_uuid(), v_recurso_id, 'Inserir Subgrupo da Apólice', 'apolices.subgrupos.inserir', true),
            (gen_random_uuid(), v_recurso_id, 'Alterar Subgrupo da Apólice', 'apolices.subgrupos.alterar', true),
            (gen_random_uuid(), v_recurso_id, 'Inativar Subgrupo da Apólice', 'apolices.subgrupos.inativar', true)
        ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome;
    END IF;
END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM seguranca.permissao WHERE codigo IN (
    'apolices.subgrupos.inserir',
    'apolices.subgrupos.alterar',
    'apolices.subgrupos.inativar'
);
            ");
        }
    }
}
