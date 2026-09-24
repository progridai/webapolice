using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoverApoliceSubestipulanteModulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_apolice_vida_apolice_subestipulante_modulo_apolice_subestip",
                schema: "seguro",
                table: "apolice_vida");

            migrationBuilder.DropTable(
                name: "apolice_subestipulante_modulo",
                schema: "seguro");

            migrationBuilder.DropIndex(
                name: "ix_apolice_vida_apolice_subestipulante_modulo_id",
                schema: "seguro",
                table: "apolice_vida");

            migrationBuilder.DropColumn(
                name: "apolice_subestipulante_modulo_id",
                schema: "seguro",
                table: "apolice_vida");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "apolice_subestipulante_modulo_id",
                schema: "seguro",
                table: "apolice_vida",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "apolice_subestipulante_modulo",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    apolice_subestipulante_id = table.Column<long>(type: "bigint", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    data_fim = table.Column<DateOnly>(type: "date", nullable: true),
                    data_inicio = table.Column<DateOnly>(type: "date", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modulo_id = table.Column<long>(type: "bigint", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_apolice_subestipulante_modulo", x => x.id);
                    table.ForeignKey(
                        name: "fk_apolice_subestipulante_modulo_apolice_subestipulante_apolic",
                        column: x => x.apolice_subestipulante_id,
                        principalSchema: "seguro",
                        principalTable: "apolice_subestipulante",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_apolice_vida_apolice_subestipulante_modulo_id",
                schema: "seguro",
                table: "apolice_vida",
                column: "apolice_subestipulante_modulo_id");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_subestipulante_modulo_apolice_subestipulante_id_mod",
                schema: "seguro",
                table: "apolice_subestipulante_modulo",
                columns: new[] { "apolice_subestipulante_id", "modulo_id" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.AddForeignKey(
                name: "fk_apolice_vida_apolice_subestipulante_modulo_apolice_subestip",
                schema: "seguro",
                table: "apolice_vida",
                column: "apolice_subestipulante_modulo_id",
                principalSchema: "seguro",
                principalTable: "apolice_subestipulante_modulo",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
