using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Cadastro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoverSubestipulanteDeClienteVinculo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "subestipulante_id",
                schema: "cadastro",
                table: "cliente_vinculo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "subestipulante_id",
                schema: "cadastro",
                table: "cliente_vinculo",
                type: "bigint",
                nullable: true);
        }
    }
}
