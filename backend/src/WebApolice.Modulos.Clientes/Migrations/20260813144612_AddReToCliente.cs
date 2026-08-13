using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Clientes.Migrations
{
    /// <inheritdoc />
    public partial class AddReToCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "re",
                schema: "cadastro",
                table: "cliente",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "re",
                schema: "cadastro",
                table: "cliente");
        }
    }
}
