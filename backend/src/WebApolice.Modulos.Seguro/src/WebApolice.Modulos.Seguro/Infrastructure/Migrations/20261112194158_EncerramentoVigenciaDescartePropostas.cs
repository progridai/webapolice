using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EncerramentoVigenciaDescartePropostas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "cancelamento_solicitado_em",
                schema: "seguro",
                table: "proposta",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "data_cancelamento_vigencia",
                schema: "seguro",
                table: "proposta",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "descartada_em",
                schema: "seguro",
                table: "proposta",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$ BEGIN
                  IF EXISTS(SELECT 1 FROM seguro.proposta WHERE descartada_em IS NOT NULL OR cancelamento_solicitado_em IS NOT NULL OR data_cancelamento_vigencia IS NOT NULL) THEN
                    RAISE EXCEPTION 'Rollback bloqueado: existem descartes ou cancelamentos solicitados.';
                  END IF;
                END $$;
                """);
            migrationBuilder.DropColumn(
                name: "cancelamento_solicitado_em",
                schema: "seguro",
                table: "proposta");

            migrationBuilder.DropColumn(
                name: "data_cancelamento_vigencia",
                schema: "seguro",
                table: "proposta");

            migrationBuilder.DropColumn(
                name: "descartada_em",
                schema: "seguro",
                table: "proposta");
        }
    }
}
