using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguranca.Migrations
{
    /// <inheritdoc />
    public partial class VinculaAdminUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO seguranca.usuario_perfil (usuario_id, perfil_id, created_at)
                SELECT u.id, p.id, NOW()
                FROM seguranca.usuario u
                CROSS JOIN seguranca.perfil p
                WHERE (u.username = 'admin' OR u.email = 'admin@progridai.com')
                  AND p.codigo = 'ADMINISTRADOR'
                  AND NOT EXISTS (
                      SELECT 1 FROM seguranca.usuario_perfil up2 
                      WHERE up2.usuario_id = u.id AND up2.perfil_id = p.id
                  );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM seguranca.usuario_perfil
                WHERE perfil_id IN (SELECT id FROM seguranca.perfil WHERE codigo = 'ADMINISTRADOR')
                  AND usuario_id IN (SELECT id FROM seguranca.usuario WHERE username = 'admin' OR email = 'admin@progridai.com');
            ");
        }
    }
}
