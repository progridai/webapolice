using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GestaoAgendamentosPropostas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "cancelada_em",
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
                  IF EXISTS(SELECT 1 FROM seguro.proposta WHERE cancelada_em IS NOT NULL) THEN
                    RAISE EXCEPTION 'Rollback bloqueado: existem agendamentos cancelados.';
                  END IF;
                END $$;
                """);
            migrationBuilder.DropColumn(
                name: "cancelada_em",
                schema: "seguro",
                table: "proposta");
        }
    }
}
