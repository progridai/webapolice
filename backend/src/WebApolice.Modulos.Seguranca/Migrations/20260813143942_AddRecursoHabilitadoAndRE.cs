using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguranca.Migrations
{
    /// <inheritdoc />
    public partial class AddRecursoHabilitadoAndRE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "habilitado",
                schema: "seguranca",
                table: "recurso",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql(@"
                INSERT INTO seguranca.recurso (public_id, modulo_id, codigo, nome, descricao, ordem, ativo, habilitado, created_at, updated_at)
                SELECT gen_random_uuid(), id, 'RE', 'RE', 'Registro de Empregado', 10, true, true, now(), now()
                FROM seguranca.modulo
                WHERE codigo = 'CLIENTES';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM seguranca.recurso
                WHERE codigo = 'RE' AND modulo_id = (SELECT id FROM seguranca.modulo WHERE codigo = 'CLIENTES');
            ");

            migrationBuilder.DropColumn(
                name: "habilitado",
                schema: "seguranca",
                table: "recurso");
        }
    }
}
