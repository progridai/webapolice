using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfirmacaoVigenciaPropostas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "motivo_pendencia_vigencia", schema: "seguro", table: "proposta", type: "character varying(1000)", maxLength: 1000, nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "pendencia_vigencia_em", schema: "seguro", table: "proposta", type: "timestamp with time zone", nullable: true);
            migrationBuilder.AddColumn<DateTime>(
                name: "confirmada_em",
                schema: "seguro",
                table: "proposta",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "data_fim_vigencia",
                schema: "seguro",
                table: "proposta",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "data_inicio_vigencia",
                schema: "seguro",
                table: "proposta",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "substituida_em",
                schema: "seguro",
                table: "proposta",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_proposta_nativa_vigente_vida",
                schema: "seguro",
                table: "proposta",
                column: "apolice_vida_id",
                unique: true,
                filter: "legado_id IS NULL AND vigente AND deleted_at IS NULL AND apolice_vida_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$ BEGIN
                  IF EXISTS(SELECT 1 FROM seguro.proposta WHERE confirmada_em IS NOT NULL OR substituida_em IS NOT NULL OR data_inicio_vigencia IS NOT NULL OR data_fim_vigencia IS NOT NULL OR motivo_pendencia_vigencia IS NOT NULL OR pendencia_vigencia_em IS NOT NULL) THEN
                    RAISE EXCEPTION 'Há confirmações, vigências ou pendências registradas. Preserve os dados antes de reverter.';
                  END IF;
                END $$;
                """);
            migrationBuilder.DropColumn(name: "motivo_pendencia_vigencia", schema: "seguro", table: "proposta");
            migrationBuilder.DropColumn(name: "pendencia_vigencia_em", schema: "seguro", table: "proposta");
            migrationBuilder.DropIndex(
                name: "ux_proposta_nativa_vigente_vida",
                schema: "seguro",
                table: "proposta");

            migrationBuilder.DropColumn(
                name: "confirmada_em",
                schema: "seguro",
                table: "proposta");

            migrationBuilder.DropColumn(
                name: "data_fim_vigencia",
                schema: "seguro",
                table: "proposta");

            migrationBuilder.DropColumn(
                name: "data_inicio_vigencia",
                schema: "seguro",
                table: "proposta");

            migrationBuilder.DropColumn(
                name: "substituida_em",
                schema: "seguro",
                table: "proposta");
        }
    }
}
