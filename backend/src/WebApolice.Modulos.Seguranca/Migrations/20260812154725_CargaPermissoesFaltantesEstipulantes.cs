using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguranca.Migrations
{
    /// <inheritdoc />
    public partial class CargaPermissoesFaltantesEstipulantes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var timestamp = "'2026-08-12 12:00:00Z'";

            // Adiciona as permissões alterar, inativar, reativar
            migrationBuilder.Sql($@"
                INSERT INTO seguranca.permissao (public_id, recurso_id, codigo, nome, descricao, ativo, created_at, updated_at)
                SELECT 'f4eebc99-9c0b-4ef8-bb6d-6bb9bd380f53', r.id, 'estipulantes.alterar', 'Alterar estipulantes', 'Permite editar os dados de um estipulante existente', true, {timestamp}, {timestamp}
                FROM seguranca.recurso r WHERE r.codigo = 'ESTIPULANTES';

                INSERT INTO seguranca.permissao (public_id, recurso_id, codigo, nome, descricao, ativo, created_at, updated_at)
                SELECT 'f5eebc99-9c0b-4ef8-bb6d-6bb9bd380f64', r.id, 'estipulantes.inativar', 'Inativar estipulantes', 'Permite inativar um estipulante', true, {timestamp}, {timestamp}
                FROM seguranca.recurso r WHERE r.codigo = 'ESTIPULANTES';

                INSERT INTO seguranca.permissao (public_id, recurso_id, codigo, nome, descricao, ativo, created_at, updated_at)
                SELECT 'f6eebc99-9c0b-4ef8-bb6d-6bb9bd380f75', r.id, 'estipulantes.reativar', 'Reativar estipulantes', 'Permite reativar um estipulante', true, {timestamp}, {timestamp}
                FROM seguranca.recurso r WHERE r.codigo = 'ESTIPULANTES';
            ");

            // Atribui as permissões ao perfil ADMINISTRATIVO
            migrationBuilder.Sql($@"
                INSERT INTO seguranca.perfil_permissao (perfil_id, permissao_id, atribuido_por_usuario_id, created_at)
                SELECT perfil.id, permissao.id, NULL, {timestamp}
                FROM seguranca.perfil perfil
                CROSS JOIN seguranca.permissao permissao
                WHERE perfil.codigo = 'ADMINISTRATIVO'
                  AND permissao.codigo IN ('estipulantes.alterar', 'estipulantes.inativar', 'estipulantes.reativar');
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM seguranca.perfil_permissao
                WHERE permissao_id IN (SELECT id FROM seguranca.permissao WHERE codigo IN ('estipulantes.alterar', 'estipulantes.inativar', 'estipulantes.reativar'));
            ");

            migrationBuilder.Sql(@"
                DELETE FROM seguranca.permissao WHERE codigo IN ('estipulantes.alterar', 'estipulantes.inativar', 'estipulantes.reativar');
            ");
        }
    }
}
