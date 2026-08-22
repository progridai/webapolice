using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguranca.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarPermissoesApoliceSubestipulante : Migration
    {
        /// <inheritdoc />
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
            (gen_random_uuid(), v_recurso_id, 'Inserir Subestipulante', 'apolices.subestipulantes.inserir', true),
            (gen_random_uuid(), v_recurso_id, 'Alterar Subestipulante', 'apolices.subestipulantes.alterar', true),
            (gen_random_uuid(), v_recurso_id, 'Inativar Subestipulante', 'apolices.subestipulantes.inativar', true)
        ON CONFLICT (codigo) DO UPDATE SET nome = EXCLUDED.nome;
    END IF;
END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM seguranca.permissao WHERE codigo IN ('apolices.subestipulantes.inserir', 'apolices.subestipulantes.alterar', 'apolices.subestipulantes.inativar');
            ");
        }
    }
}
