using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PreenchimentoRascunhoPropostas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "justificativa_ajuste_premio",
                schema: "seguro",
                table: "proposta_cobertura",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "premio_ajustado",
                schema: "seguro",
                table: "proposta_cobertura",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "premio_conjuge_inicial",
                schema: "seguro",
                table: "proposta_cobertura",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "premio_titular_inicial",
                schema: "seguro",
                table: "proposta_cobertura",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "legado_id",
                schema: "seguro",
                table: "proposta_beneficiario",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "parentesco_registrado",
                schema: "seguro",
                table: "proposta_beneficiario",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "public_id",
                schema: "seguro",
                table: "proposta_beneficiario",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<int>(
                name: "revisao_preenchimento",
                schema: "seguro",
                table: "proposta",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_proposta_beneficiario_public_id",
                schema: "seguro",
                table: "proposta_beneficiario",
                column: "public_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $guard$
                BEGIN
                  IF EXISTS (SELECT 1 FROM seguro.proposta WHERE revisao_preenchimento <> 0)
                     OR EXISTS (SELECT 1 FROM seguro.proposta_beneficiario WHERE legado_id IS NULL OR parentesco_registrado IS NOT NULL)
                     OR EXISTS (SELECT 1 FROM seguro.proposta_cobertura WHERE premio_ajustado OR justificativa_ajuste_premio IS NOT NULL OR premio_titular_inicial IS NOT NULL OR premio_conjuge_inicial IS NOT NULL) THEN
                    RAISE EXCEPTION 'Há preenchimentos de propostas. Preserve os dados antes de reverter a migration.';
                  END IF;
                END $guard$;
                """);
            migrationBuilder.DropIndex(
                name: "ix_proposta_beneficiario_public_id",
                schema: "seguro",
                table: "proposta_beneficiario");

            migrationBuilder.DropColumn(
                name: "justificativa_ajuste_premio",
                schema: "seguro",
                table: "proposta_cobertura");

            migrationBuilder.DropColumn(
                name: "premio_ajustado",
                schema: "seguro",
                table: "proposta_cobertura");

            migrationBuilder.DropColumn(
                name: "premio_conjuge_inicial",
                schema: "seguro",
                table: "proposta_cobertura");

            migrationBuilder.DropColumn(
                name: "premio_titular_inicial",
                schema: "seguro",
                table: "proposta_cobertura");

            migrationBuilder.DropColumn(
                name: "parentesco_registrado",
                schema: "seguro",
                table: "proposta_beneficiario");

            migrationBuilder.DropColumn(
                name: "public_id",
                schema: "seguro",
                table: "proposta_beneficiario");

            migrationBuilder.DropColumn(
                name: "revisao_preenchimento",
                schema: "seguro",
                table: "proposta");

            migrationBuilder.AlterColumn<int>(
                name: "legado_id",
                schema: "seguro",
                table: "proposta_beneficiario",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
