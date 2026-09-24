using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoverApoliceSubestipulanteDeApoliceVida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_apolice_vida_apolice_subestipulante_apolice_subestipulante_~",
                schema: "seguro",
                table: "apolice_vida");

            migrationBuilder.DropIndex(
                name: "ix_apolice_vida_subestip",
                schema: "seguro",
                table: "apolice_vida");

            migrationBuilder.DropColumn(
                name: "apolice_subestipulante_id",
                schema: "seguro",
                table: "apolice_vida");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "apolice_subestipulante_id",
                schema: "seguro",
                table: "apolice_vida",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_apolice_vida_subestip",
                schema: "seguro",
                table: "apolice_vida",
                column: "apolice_subestipulante_id",
                filter: "apolice_subestipulante_id IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_apolice_vida_apolice_subestipulante_apolice_subestipulante_~",
                schema: "seguro",
                table: "apolice_vida",
                column: "apolice_subestipulante_id",
                principalSchema: "seguro",
                principalTable: "apolice_subestipulante",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
