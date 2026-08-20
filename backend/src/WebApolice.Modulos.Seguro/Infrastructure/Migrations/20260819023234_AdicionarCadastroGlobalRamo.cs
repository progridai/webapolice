using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Seguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCadastroGlobalRamo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ramo",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ramo", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_ramo_public_id",
                schema: "seguro",
                table: "ramo",
                column: "public_id",
                unique: true);

            migrationBuilder.AddColumn<long>(
                name: "ramo_id",
                schema: "seguro",
                table: "apolice_ramo",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // Migrar os dados existentes
            migrationBuilder.Sql(@"
                -- Inserir os ramos únicos a partir dos dados existentes
                INSERT INTO seguro.ramo (public_id, codigo, nome, ativo, created_at, updated_at)
                SELECT gen_random_uuid(), tipo_ramo, tipo_ramo, true, now(), now()
                FROM (
                    SELECT DISTINCT tipo_ramo 
                    FROM seguro.apolice_ramo 
                    WHERE tipo_ramo IS NOT NULL AND tipo_ramo <> ''
                ) AS u;

                -- Atualizar a chave estrangeira na tabela apolice_ramo
                UPDATE seguro.apolice_ramo ar
                SET ramo_id = r.id
                FROM seguro.ramo r
                WHERE ar.tipo_ramo = r.codigo;
            ");

            migrationBuilder.DropIndex(
                name: "ix_apolice_ramo_tipo_ramo",
                schema: "seguro",
                table: "apolice_ramo");

            migrationBuilder.DropIndex(
                name: "ux_apolice_ramo_ativo",
                schema: "seguro",
                table: "apolice_ramo");

            migrationBuilder.DropColumn(
                name: "tipo_ramo",
                schema: "seguro",
                table: "apolice_ramo");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_ramo_ramo",
                schema: "seguro",
                table: "apolice_ramo",
                column: "ramo_id");

            migrationBuilder.CreateIndex(
                name: "ux_apolice_ramo_ativo",
                schema: "seguro",
                table: "apolice_ramo",
                columns: new[] { "apolice_id", "ramo_id" },
                unique: true,
                filter: "ativo = true");

            migrationBuilder.AddForeignKey(
                name: "fk_apolice_ramo_ramo_ramo_id",
                schema: "seguro",
                table: "apolice_ramo",
                column: "ramo_id",
                principalSchema: "seguro",
                principalTable: "ramo",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_apolice_ramo_ramo_ramo_id",
                schema: "seguro",
                table: "apolice_ramo");

            migrationBuilder.DropTable(
                name: "ramo",
                schema: "seguro");

            migrationBuilder.DropIndex(
                name: "ix_apolice_ramo_ramo",
                schema: "seguro",
                table: "apolice_ramo");

            migrationBuilder.DropIndex(
                name: "ux_apolice_ramo_ativo",
                schema: "seguro",
                table: "apolice_ramo");

            migrationBuilder.DropColumn(
                name: "ramo_id",
                schema: "seguro",
                table: "apolice_ramo");

            migrationBuilder.AddColumn<string>(
                name: "tipo_ramo",
                schema: "seguro",
                table: "apolice_ramo",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_ramo_tipo_ramo",
                schema: "seguro",
                table: "apolice_ramo",
                column: "tipo_ramo");

            migrationBuilder.CreateIndex(
                name: "ux_apolice_ramo_ativo",
                schema: "seguro",
                table: "apolice_ramo",
                columns: new[] { "apolice_id", "tipo_ramo" },
                unique: true,
                filter: "ativo = true");
        }
    }
}
