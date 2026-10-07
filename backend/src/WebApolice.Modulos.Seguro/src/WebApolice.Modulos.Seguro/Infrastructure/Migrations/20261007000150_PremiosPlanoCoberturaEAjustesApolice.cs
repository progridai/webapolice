using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PremiosPlanoCoberturaEAjustesApolice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "premio_conjuge",
                schema: "seguro",
                table: "plano_cobertura",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "premio_titular",
                schema: "seguro",
                table: "plano_cobertura",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "premio_conjuge_override",
                schema: "seguro",
                table: "apolice_cobertura",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "premio_titular_override",
                schema: "seguro",
                table: "apolice_cobertura",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "public_id",
                schema: "seguro",
                table: "apolice_cobertura",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_cobertura_public_id",
                schema: "seguro",
                table: "apolice_cobertura",
                column: "public_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_apolice_cobertura_public_id",
                schema: "seguro",
                table: "apolice_cobertura");

            migrationBuilder.DropColumn(
                name: "premio_conjuge",
                schema: "seguro",
                table: "plano_cobertura");

            migrationBuilder.DropColumn(
                name: "premio_titular",
                schema: "seguro",
                table: "plano_cobertura");

            migrationBuilder.DropColumn(
                name: "premio_conjuge_override",
                schema: "seguro",
                table: "apolice_cobertura");

            migrationBuilder.DropColumn(
                name: "premio_titular_override",
                schema: "seguro",
                table: "apolice_cobertura");

            migrationBuilder.DropColumn(
                name: "public_id",
                schema: "seguro",
                table: "apolice_cobertura");
        }
    }
}
