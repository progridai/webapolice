using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarSubgrupoEModuloEmApoliceVida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "apolice_modulo_id",
                schema: "seguro",
                table: "apolice_vida",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "apolice_subgrupo_id",
                schema: "seguro",
                table: "apolice_vida",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_apolice_vida_modulo",
                schema: "seguro",
                table: "apolice_vida",
                column: "apolice_modulo_id",
                filter: "apolice_modulo_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_vida_subgrupo",
                schema: "seguro",
                table: "apolice_vida",
                column: "apolice_subgrupo_id",
                filter: "apolice_subgrupo_id IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "fk_apolice_vida_apolice_modulos_apolice_modulo_id",
                schema: "seguro",
                table: "apolice_vida",
                column: "apolice_modulo_id",
                principalSchema: "seguro",
                principalTable: "apolice_modulo",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_apolice_vida_apolice_subgrupos_apolice_subgrupo_id",
                schema: "seguro",
                table: "apolice_vida",
                column: "apolice_subgrupo_id",
                principalSchema: "seguro",
                principalTable: "apolice_subgrupo",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_apolice_vida_apolice_modulos_apolice_modulo_id",
                schema: "seguro",
                table: "apolice_vida");

            migrationBuilder.DropForeignKey(
                name: "fk_apolice_vida_apolice_subgrupos_apolice_subgrupo_id",
                schema: "seguro",
                table: "apolice_vida");

            migrationBuilder.DropIndex(
                name: "ix_apolice_vida_modulo",
                schema: "seguro",
                table: "apolice_vida");

            migrationBuilder.DropIndex(
                name: "ix_apolice_vida_subgrupo",
                schema: "seguro",
                table: "apolice_vida");

            migrationBuilder.DropColumn(
                name: "apolice_modulo_id",
                schema: "seguro",
                table: "apolice_vida");

            migrationBuilder.DropColumn(
                name: "apolice_subgrupo_id",
                schema: "seguro",
                table: "apolice_vida");
        }
    }
}
