using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Financeiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CriarConveniosCobranca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "financeiro");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.CreateTable(
                name: "convenio_cobranca",
                schema: "financeiro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    banco_id = table.Column<long>(type: "bigint", nullable: true),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    agencia = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    conta_corrente = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    nome_empresa = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    codigo_empresa = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    numero_arquivo = table.Column<int>(type: "integer", nullable: true),
                    nome_inicial_arquivo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    extensao_arquivo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    layout_arquivo = table.Column<short>(type: "smallint", nullable: true),
                    local_remessa_arquivo = table.Column<string>(type: "text", nullable: true),
                    local_retorno_arquivo = table.Column<string>(type: "text", nullable: true),
                    comunica_vindi = table.Column<bool>(type: "boolean", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    inscricao_estadual = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    est_endereco = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    est_numero = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    est_bairro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    est_complemento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    est_cep = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    est_cidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    est_uf = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    est_nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("convenio_cobranca_pkey", x => x.id);
                    table.ForeignKey("fk_convenio_cobranca_banco", x=>x.banco_id, "banco", "id", principalSchema: "core", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_convenio_cobranca_banco_id",
                schema: "financeiro",
                table: "convenio_cobranca",
                column: "banco_id");

            migrationBuilder.CreateIndex(
                name: "ix_convenio_cobranca_legado_id",
                schema: "financeiro",
                table: "convenio_cobranca",
                column: "legado_id",
                unique: true,
                filter: "(legado_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_convenio_cobranca_public_id",
                schema: "financeiro",
                table: "convenio_cobranca",
                column: "public_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "convenio_cobranca",
                schema: "financeiro");
        }
    }
}
