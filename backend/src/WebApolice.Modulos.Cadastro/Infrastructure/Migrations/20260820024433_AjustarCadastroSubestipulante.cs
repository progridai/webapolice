using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Cadastro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjustarCadastroSubestipulante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "possui_logotipo_legado",
                schema: "cadastro",
                table: "corretora",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "fk_subestipulante_pessoa_pessoa_id",
                schema: "cadastro",
                table: "subestipulante",
                column: "pessoa_id",
                principalSchema: "core",
                principalTable: "pessoa",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("ALTER TABLE cadastro.subestipulante DROP COLUMN IF EXISTS nome;");
            migrationBuilder.Sql("ALTER TABLE cadastro.subestipulante DROP COLUMN IF EXISTS razao_social;");
            migrationBuilder.Sql("ALTER TABLE cadastro.subestipulante DROP COLUMN IF EXISTS cnpj;");
            migrationBuilder.Sql("ALTER TABLE cadastro.subestipulante DROP COLUMN IF EXISTS cpf;");
            migrationBuilder.Sql("ALTER TABLE cadastro.subestipulante DROP COLUMN IF EXISTS email;");
            migrationBuilder.Sql("ALTER TABLE cadastro.subestipulante DROP COLUMN IF EXISTS telefone_comercial;");
            migrationBuilder.Sql("ALTER TABLE cadastro.subestipulante DROP COLUMN IF EXISTS telefone_celular;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_subestipulante_pessoa_pessoa_id",
                schema: "cadastro",
                table: "subestipulante");

            migrationBuilder.AlterColumn<bool>(
                name: "possui_logotipo_legado",
                schema: "cadastro",
                table: "corretora",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }
    }
}
