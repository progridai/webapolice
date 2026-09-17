using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateApoliceModulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "apolice_modulo",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    apolice_id = table.Column<long>(type: "bigint", nullable: false),
                    modulo_id = table.Column<long>(type: "bigint", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apolice_modulo", x => x.id);
                    table.ForeignKey(
                        name: "fk_apolice_modulo_apolice_id",
                        column: x => x.apolice_id,
                        principalSchema: "seguro",
                        principalTable: "apolice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_apolice_modulo_apolice_id",
                schema: "seguro",
                table: "apolice_modulo",
                column: "apolice_id");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_modulo_modulo_id",
                schema: "seguro",
                table: "apolice_modulo",
                column: "modulo_id");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_modulo_public_id",
                schema: "seguro",
                table: "apolice_modulo",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_apolice_modulo_apolice_id_modulo_id",
                schema: "seguro",
                table: "apolice_modulo",
                columns: new[] { "apolice_id", "modulo_id" },
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "apolice_modulo",
                schema: "seguro");
        }
    }
}
