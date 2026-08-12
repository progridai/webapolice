using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguranca.Migrations
{
    /// <inheritdoc />
    public partial class CargaInicialEstipulantes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var timestamp = "'2026-08-12 00:00:00Z'";

            // Adiciona o módulo ESTIPULANTES
            migrationBuilder.Sql($@"
                INSERT INTO seguranca.modulo (public_id, codigo, nome, descricao, icone, ordem, ativo, created_at, updated_at)
                VALUES ('e0eebc99-9c0b-4ef8-bb6d-6bb9bd380e11', 'ESTIPULANTES', 'Estipulantes', 'Gestão de Estipulantes e Subestipulantes', 'users', 20, true, {timestamp}, {timestamp});
            ");

            // Adiciona o recurso
            migrationBuilder.Sql($@"
                INSERT INTO seguranca.recurso (public_id, modulo_id, codigo, nome, descricao, rota_frontend, ordem, ativo, created_at, updated_at)
                SELECT 'f1eebc99-9c0b-4ef8-bb6d-6bb9bd380f22', m.id, 'ESTIPULANTES', 'Estipulantes', 'Cadastro de estipulantes', '/estipulantes', 10, true, {timestamp}, {timestamp}
                FROM seguranca.modulo m
                WHERE m.codigo = 'ESTIPULANTES';
            ");

            // Adiciona a permissão visualizar
            migrationBuilder.Sql($@"
                INSERT INTO seguranca.permissao (public_id, recurso_id, codigo, nome, descricao, ativo, created_at, updated_at)
                SELECT 'f2eebc99-9c0b-4ef8-bb6d-6bb9bd380f31', r.id, 'estipulantes.visualizar', 'Visualizar estipulantes', 'Permite consultar a listagem e detalhes', true, {timestamp}, {timestamp}
                FROM seguranca.recurso r WHERE r.codigo = 'ESTIPULANTES';
            ");

            // Atribui a permissão ao perfil ADMINISTRATIVO
            migrationBuilder.Sql($@"
                INSERT INTO seguranca.perfil_permissao (perfil_id, permissao_id, atribuido_por_usuario_id, created_at)
                SELECT perfil.id, permissao.id, NULL, {timestamp}
                FROM seguranca.perfil perfil
                CROSS JOIN seguranca.permissao permissao
                WHERE perfil.codigo = 'ADMINISTRATIVO'
                  AND permissao.codigo = 'estipulantes.visualizar';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM seguranca.perfil_permissao
                WHERE permissao_id IN (SELECT id FROM seguranca.permissao WHERE codigo = 'estipulantes.visualizar');
            ");

            migrationBuilder.Sql(@"
                DELETE FROM seguranca.permissao WHERE codigo = 'estipulantes.visualizar';
            ");

            migrationBuilder.Sql(@"
                DELETE FROM seguranca.recurso WHERE codigo = 'ESTIPULANTES';
            ");

            migrationBuilder.Sql(@"
                DELETE FROM seguranca.modulo WHERE codigo = 'ESTIPULANTES';
            ");
        }
    }
}
