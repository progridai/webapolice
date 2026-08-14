using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Integracao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "integracao");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.CreateTable(
                name: "referencia_externa",
                schema: "integracao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sistema = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entidade_tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entidade_id = table.Column<long>(type: "bigint", nullable: false),
                    chave_externa = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    dados = table.Column<string>(type: "jsonb", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("referencia_externa_pkey", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_referencia_externa_entidade",
                schema: "integracao",
                table: "referencia_externa",
                columns: new[] { "entidade_tipo", "entidade_id" });

            migrationBuilder.CreateIndex(
                name: "referencia_externa_sistema_entidade_tipo_chave_externa_key",
                schema: "integracao",
                table: "referencia_externa",
                columns: new[] { "sistema", "entidade_tipo", "chave_externa" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "referencia_externa",
                schema: "integracao");
        }
    }
}
