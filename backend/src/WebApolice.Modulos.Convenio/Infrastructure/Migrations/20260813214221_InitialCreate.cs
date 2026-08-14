using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Convenio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "convenio");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.CreateTable(
                name: "corsan_cliente",
                schema: "convenio",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    cliente_vinculo_id = table.Column<long>(type: "bigint", nullable: true),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: false),
                    empresa = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    rubrica = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    grupo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    funcionario = table.Column<bool>(type: "boolean", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("corsan_cliente_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "corsan_proposta",
                schema: "convenio",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    proposta_id = table.Column<long>(type: "bigint", nullable: false),
                    cliente_id = table.Column<long>(type: "bigint", nullable: true),
                    cliente_vinculo_id = table.Column<long>(type: "bigint", nullable: true),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    empresa = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    rubrica = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    grupo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("corsan_proposta_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "siape_orgao",
                schema: "convenio",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("siape_orgao_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "siape_parametro",
                schema: "convenio",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    empresa = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cgc = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    cgc_limpo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    rubrica = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    comando = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    custo_linha = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    calculo_parametro = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("siape_parametro_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "siape_cliente",
                schema: "convenio",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    cliente_vinculo_id = table.Column<long>(type: "bigint", nullable: true),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: false),
                    siape = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    orgao_id = table.Column<long>(type: "bigint", nullable: true),
                    categoria = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    setor = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    instituto = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    agencia = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    funcao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    contrato = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    digito_verificador = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    instituidor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("siape_cliente_pkey", x => x.id);
                    table.ForeignKey(
                        name: "siape_cliente_orgao_id_fkey",
                        column: x => x.orgao_id,
                        principalSchema: "convenio",
                        principalTable: "siape_orgao",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_corsan_cliente_cliente",
                schema: "convenio",
                table: "corsan_cliente",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_corsan_proposta_cliente",
                schema: "convenio",
                table: "corsan_proposta",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ux_corsan_proposta",
                schema: "convenio",
                table: "corsan_proposta",
                column: "proposta_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_siape_cliente_cliente",
                schema: "convenio",
                table: "siape_cliente",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_siape_cliente_orgao_id",
                schema: "convenio",
                table: "siape_cliente",
                column: "orgao_id");

            migrationBuilder.CreateIndex(
                name: "ix_siape_cliente_siape",
                schema: "convenio",
                table: "siape_cliente",
                column: "siape");

            migrationBuilder.CreateIndex(
                name: "ux_siape_orgao_legado",
                schema: "convenio",
                table: "siape_orgao",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ux_siape_parametro_legado",
                schema: "convenio",
                table: "siape_parametro",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "corsan_cliente",
                schema: "convenio");

            migrationBuilder.DropTable(
                name: "corsan_proposta",
                schema: "convenio");

            migrationBuilder.DropTable(
                name: "siape_cliente",
                schema: "convenio");

            migrationBuilder.DropTable(
                name: "siape_parametro",
                schema: "convenio");

            migrationBuilder.DropTable(
                name: "siape_orgao",
                schema: "convenio");
        }
    }
}
