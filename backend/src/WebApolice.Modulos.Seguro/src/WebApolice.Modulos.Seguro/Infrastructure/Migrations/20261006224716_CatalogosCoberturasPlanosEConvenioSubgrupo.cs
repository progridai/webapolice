using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CatalogosCoberturasPlanosEConvenioSubgrupo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "public_id",
                schema: "seguro",
                table: "plano",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "public_id",
                schema: "seguro",
                table: "cobertura",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<long>(
                name: "convenio_cobranca_id",
                schema: "seguro",
                table: "apolice_subgrupo",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddForeignKey("fk_apolice_subgrupo_convenio_cobranca", "apolice_subgrupo", "convenio_cobranca_id", "convenio_cobranca", schema: "seguro", principalSchema: "financeiro", principalColumn: "id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.CreateTable(
                name: "plano_cobertura",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    plano_id = table.Column<long>(type: "bigint", nullable: false),
                    cobertura_id = table.Column<long>(type: "bigint", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plano_cobertura", x => x.id);
                    table.ForeignKey(
                        name: "fk_plano_cobertura_cobertura_cobertura_id",
                        column: x => x.cobertura_id,
                        principalSchema: "seguro",
                        principalTable: "cobertura",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_plano_cobertura_plano_plano_id",
                        column: x => x.plano_id,
                        principalSchema: "seguro",
                        principalTable: "plano",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_plano_public_id",
                schema: "seguro",
                table: "plano",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cobertura_public_id",
                schema: "seguro",
                table: "cobertura",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_apolice_subgrupo_convenio_cobranca_id",
                schema: "seguro",
                table: "apolice_subgrupo",
                column: "convenio_cobranca_id");

            migrationBuilder.CreateIndex(
                name: "ix_plano_cobertura_cobertura_id",
                schema: "seguro",
                table: "plano_cobertura",
                column: "cobertura_id");

            migrationBuilder.CreateIndex(
                name: "ix_plano_cobertura_plano_id_cobertura_id",
                schema: "seguro",
                table: "plano_cobertura",
                columns: new[] { "plano_id", "cobertura_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("fk_apolice_subgrupo_convenio_cobranca", "apolice_subgrupo", "seguro");
            migrationBuilder.DropTable(
                name: "plano_cobertura",
                schema: "seguro");

            migrationBuilder.DropIndex(
                name: "ix_plano_public_id",
                schema: "seguro",
                table: "plano");

            migrationBuilder.DropIndex(
                name: "ix_cobertura_public_id",
                schema: "seguro",
                table: "cobertura");

            migrationBuilder.DropIndex(
                name: "ix_apolice_subgrupo_convenio_cobranca_id",
                schema: "seguro",
                table: "apolice_subgrupo");

            migrationBuilder.DropColumn(
                name: "public_id",
                schema: "seguro",
                table: "plano");

            migrationBuilder.DropColumn(
                name: "public_id",
                schema: "seguro",
                table: "cobertura");

            migrationBuilder.DropColumn(
                name: "convenio_cobranca_id",
                schema: "seguro",
                table: "apolice_subgrupo");
        }
    }
}
