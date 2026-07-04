using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Auditoria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "auditoria");

            migrationBuilder.CreateTable(
                name: "registros_auditoria",
                schema: "auditoria",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data_hora_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario_id_externo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    usuario_nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    acao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modulo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    recurso = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    recurso_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    resultado = table.Column<int>(type: "integer", nullable: false),
                    trace_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    endereco_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    origem = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    dados_anteriores = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    dados_posteriores = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    metadados = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    mensagem_erro = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_registros_auditoria", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_registros_auditoria_correlation_id",
                schema: "auditoria",
                table: "registros_auditoria",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_registros_auditoria_data_hora_utc",
                schema: "auditoria",
                table: "registros_auditoria",
                column: "data_hora_utc");

            migrationBuilder.CreateIndex(
                name: "ix_registros_auditoria_modulo_recurso_recurso_id",
                schema: "auditoria",
                table: "registros_auditoria",
                columns: new[] { "modulo", "recurso", "recurso_id" });

            migrationBuilder.CreateIndex(
                name: "ix_registros_auditoria_resultado",
                schema: "auditoria",
                table: "registros_auditoria",
                column: "resultado");

            migrationBuilder.CreateIndex(
                name: "ix_registros_auditoria_trace_id",
                schema: "auditoria",
                table: "registros_auditoria",
                column: "trace_id");

            migrationBuilder.CreateIndex(
                name: "ix_registros_auditoria_usuario_id_externo_data_hora_utc",
                schema: "auditoria",
                table: "registros_auditoria",
                columns: new[] { "usuario_id_externo", "data_hora_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "registros_auditoria",
                schema: "auditoria");
        }
    }
}
