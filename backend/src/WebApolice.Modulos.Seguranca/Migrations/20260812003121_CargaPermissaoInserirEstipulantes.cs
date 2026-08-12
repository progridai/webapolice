using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguranca.Migrations
{
    /// <inheritdoc />
    public partial class CargaPermissaoInserirEstipulantes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var timestamp = "'2026-08-12 00:30:00Z'";

            // Adiciona a permissão inserir
            migrationBuilder.Sql($@"
                INSERT INTO seguranca.permissao (public_id, recurso_id, codigo, nome, descricao, ativo, created_at, updated_at)
                SELECT 'f3eebc99-9c0b-4ef8-bb6d-6bb9bd380f42', r.id, 'estipulantes.inserir', 'Criar estipulantes', 'Permite cadastrar um novo estipulante', true, {timestamp}, {timestamp}
                FROM seguranca.recurso r WHERE r.codigo = 'ESTIPULANTES';
            ");

            // Atribui a permissão ao perfil ADMINISTRATIVO
            migrationBuilder.Sql($@"
                INSERT INTO seguranca.perfil_permissao (perfil_id, permissao_id, atribuido_por_usuario_id, created_at)
                SELECT perfil.id, permissao.id, NULL, {timestamp}
                FROM seguranca.perfil perfil
                CROSS JOIN seguranca.permissao permissao
                WHERE perfil.codigo = 'ADMINISTRATIVO'
                  AND permissao.codigo = 'estipulantes.inserir';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM seguranca.perfil_permissao
                WHERE permissao_id IN (SELECT id FROM seguranca.permissao WHERE codigo = 'estipulantes.inserir');
            ");

            migrationBuilder.Sql(@"
                DELETE FROM seguranca.permissao WHERE codigo = 'estipulantes.inserir';
            ");
        }
    }
}
