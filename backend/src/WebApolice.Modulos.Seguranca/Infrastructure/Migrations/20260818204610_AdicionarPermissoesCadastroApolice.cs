using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguranca.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarPermissoesCadastroApolice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
DECLARE
    v_recurso_id bigint;
BEGIN
    SELECT id INTO v_recurso_id FROM seguranca.recurso WHERE codigo = 'APOLICES';

    IF v_recurso_id IS NOT NULL THEN
        INSERT INTO seguranca.permissao (public_id, recurso_id, nome, codigo, ativo)
        VALUES 
            ('81820611-0000-0000-0000-000000000004', v_recurso_id, 'Inserir', 'apolices.inserir', true),
            ('81820611-0000-0000-0000-000000000005', v_recurso_id, 'Alterar', 'apolices.alterar', true)
        ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome;
    END IF;
END $$;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM seguranca.permissao WHERE codigo IN ('apolices.inserir', 'apolices.alterar');
            ");
        }
    }
}
