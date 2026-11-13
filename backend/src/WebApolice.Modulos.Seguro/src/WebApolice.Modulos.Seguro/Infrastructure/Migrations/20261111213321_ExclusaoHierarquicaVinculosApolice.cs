using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExclusaoHierarquicaVinculosApolice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_apolice_modulo_plano_apolice_modulo_id",
                schema: "seguro",
                table: "apolice_modulo_plano");

            migrationBuilder.DropIndex(
                name: "ix_apolice_modulo_cobertura_apolice_modulo_plano_id_cobertura_",
                schema: "seguro",
                table: "apolice_modulo_cobertura");

            migrationBuilder.AddColumn<Guid>(
                name: "public_id",
                schema: "seguro",
                table: "apolice_subestipulante",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                schema: "seguro",
                table: "apolice_ramo",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "public_id",
                schema: "seguro",
                table: "apolice_ramo",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                schema: "seguro",
                table: "apolice_produto",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "public_id",
                schema: "seguro",
                table: "apolice_produto",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                schema: "seguro",
                table: "apolice_plano",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "public_id",
                schema: "seguro",
                table: "apolice_plano",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                schema: "seguro",
                table: "apolice_modulo_plano",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                schema: "seguro",
                table: "apolice_modulo_cobertura",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                schema: "seguro",
                table: "apolice_cobertura",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_apolice_subestipulante_public_id",
                schema: "seguro",
                table: "apolice_subestipulante",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_apolice_ramo_public_id",
                schema: "seguro",
                table: "apolice_ramo",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_apolice_produto_public_id",
                schema: "seguro",
                table: "apolice_produto",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_apolice_plano_public_id",
                schema: "seguro",
                table: "apolice_plano",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_apolice_modulo_plano_apolice_modulo_id",
                schema: "seguro",
                table: "apolice_modulo_plano",
                column: "apolice_modulo_id",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_modulo_cobertura_apolice_modulo_plano_id_cobertura_",
                schema: "seguro",
                table: "apolice_modulo_cobertura",
                columns: new[] { "apolice_modulo_plano_id", "cobertura_id" },
                unique: true,
                filter: "deleted_at IS NULL");
            migrationBuilder.Sql(HierarquiaVinculosSql.Up);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(HierarquiaVinculosSql.Down);
            migrationBuilder.DropIndex(
                name: "ix_apolice_subestipulante_public_id",
                schema: "seguro",
                table: "apolice_subestipulante");

            migrationBuilder.DropIndex(
                name: "ix_apolice_ramo_public_id",
                schema: "seguro",
                table: "apolice_ramo");

            migrationBuilder.DropIndex(
                name: "ix_apolice_produto_public_id",
                schema: "seguro",
                table: "apolice_produto");

            migrationBuilder.DropIndex(
                name: "ix_apolice_plano_public_id",
                schema: "seguro",
                table: "apolice_plano");

            migrationBuilder.DropIndex(
                name: "ix_apolice_modulo_plano_apolice_modulo_id",
                schema: "seguro",
                table: "apolice_modulo_plano");

            migrationBuilder.DropIndex(
                name: "ix_apolice_modulo_cobertura_apolice_modulo_plano_id_cobertura_",
                schema: "seguro",
                table: "apolice_modulo_cobertura");

            migrationBuilder.DropColumn(
                name: "public_id",
                schema: "seguro",
                table: "apolice_subestipulante");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "seguro",
                table: "apolice_ramo");

            migrationBuilder.DropColumn(
                name: "public_id",
                schema: "seguro",
                table: "apolice_ramo");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "seguro",
                table: "apolice_produto");

            migrationBuilder.DropColumn(
                name: "public_id",
                schema: "seguro",
                table: "apolice_produto");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "seguro",
                table: "apolice_plano");

            migrationBuilder.DropColumn(
                name: "public_id",
                schema: "seguro",
                table: "apolice_plano");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "seguro",
                table: "apolice_modulo_plano");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "seguro",
                table: "apolice_modulo_cobertura");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "seguro",
                table: "apolice_cobertura");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_modulo_plano_apolice_modulo_id",
                schema: "seguro",
                table: "apolice_modulo_plano",
                column: "apolice_modulo_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_apolice_modulo_cobertura_apolice_modulo_plano_id_cobertura_",
                schema: "seguro",
                table: "apolice_modulo_cobertura",
                columns: new[] { "apolice_modulo_plano_id", "cobertura_id" },
                unique: true);
        }
    }
}
