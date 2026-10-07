using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PlanoECoberturasExclusivosModuloApolice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "apolice_modulo_plano",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    apolice_modulo_id = table.Column<long>(type: "bigint", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ramo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    paga = table.Column<bool>(type: "boolean", nullable: true),
                    reajuste = table.Column<bool>(type: "boolean", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_apolice_modulo_plano", x => x.id);
                    table.CheckConstraint("ck_modulo_plano_nome", "length(btrim(nome)) > 0");
                    table.ForeignKey(
                        name: "fk_apolice_modulo_plano_apolice_modulo_apolice_modulo_id",
                        column: x => x.apolice_modulo_id,
                        principalSchema: "seguro",
                        principalTable: "apolice_modulo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "apolice_modulo_cobertura",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    apolice_modulo_plano_id = table.Column<long>(type: "bigint", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    nome_reduzido = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    basica = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    reajuste = table.Column<bool>(type: "boolean", nullable: true),
                    premio_titular = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    premio_conjuge = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_apolice_modulo_cobertura", x => x.id);
                    table.CheckConstraint("ck_modulo_cobertura_nome", "length(btrim(nome)) > 0");
                    table.CheckConstraint("ck_modulo_cobertura_premios", "premio_titular >= 0 AND premio_conjuge >= 0");
                    table.ForeignKey(
                        name: "fk_apolice_modulo_cobertura_apolice_modulo_plano_apolice_modul",
                        column: x => x.apolice_modulo_plano_id,
                        principalSchema: "seguro",
                        principalTable: "apolice_modulo_plano",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_apolice_modulo_cobertura_apolice_modulo_plano_id",
                schema: "seguro",
                table: "apolice_modulo_cobertura",
                column: "apolice_modulo_plano_id");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_modulo_cobertura_public_id",
                schema: "seguro",
                table: "apolice_modulo_cobertura",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_apolice_modulo_plano_apolice_modulo_id",
                schema: "seguro",
                table: "apolice_modulo_plano",
                column: "apolice_modulo_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_apolice_modulo_plano_public_id",
                schema: "seguro",
                table: "apolice_modulo_plano",
                column: "public_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "apolice_modulo_cobertura",
                schema: "seguro");

            migrationBuilder.DropTable(
                name: "apolice_modulo_plano",
                schema: "seguro");
        }
    }
}
