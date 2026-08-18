using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Seguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InicializarApolicesEProposta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "apolice_id",
                schema: "seguro",
                table: "proposta",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "apolice_vida_id",
                schema: "seguro",
                table: "proposta",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "apolice",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    estipulante_id = table.Column<long>(type: "bigint", nullable: false),
                    seguradora_id = table.Column<long>(type: "bigint", nullable: false),
                    corretora_id = table.Column<long>(type: "bigint", nullable: true),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    data_inicio_vigencia = table.Column<DateOnly>(type: "date", nullable: false),
                    data_fim_vigencia = table.Column<DateOnly>(type: "date", nullable: true),
                    data_aniversario = table.Column<DateOnly>(type: "date", nullable: true),
                    apolice_origem_id = table.Column<long>(type: "bigint", nullable: true),
                    versao = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValue: "ativa"),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apolice", x => x.id);
                    table.ForeignKey(
                        name: "FK_apolice_apolice_apolice_origem_id",
                        column: x => x.apolice_origem_id,
                        principalSchema: "seguro",
                        principalTable: "apolice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "apolice_ramo",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    apolice_id = table.Column<long>(type: "bigint", nullable: false),
                    tipo_ramo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    numero_apolice = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    iof_percentual = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apolice_ramo", x => x.id);
                    table.ForeignKey(
                        name: "FK_apolice_ramo_apolice_apolice_id",
                        column: x => x.apolice_id,
                        principalSchema: "seguro",
                        principalTable: "apolice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "apolice_subestipulante",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    apolice_id = table.Column<long>(type: "bigint", nullable: false),
                    subestipulante_id = table.Column<long>(type: "bigint", nullable: false),
                    data_inicio = table.Column<DateOnly>(type: "date", nullable: true),
                    data_fim = table.Column<DateOnly>(type: "date", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apolice_subestipulante", x => x.id);
                    table.ForeignKey(
                        name: "FK_apolice_subestipulante_apolice_apolice_id",
                        column: x => x.apolice_id,
                        principalSchema: "seguro",
                        principalTable: "apolice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "apolice_vida",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    apolice_id = table.Column<long>(type: "bigint", nullable: false),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    cliente_vinculo_id = table.Column<long>(type: "bigint", nullable: true),
                    apolice_subestipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    data_inicio_vigencia = table.Column<DateOnly>(type: "date", nullable: true),
                    data_fim_vigencia = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValue: "ativa"),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    origem = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apolice_vida", x => x.id);
                    table.ForeignKey(
                        name: "FK_apolice_vida_apolice_apolice_id",
                        column: x => x.apolice_id,
                        principalSchema: "seguro",
                        principalTable: "apolice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_apolice_vida_apolice_subestipulante_apolice_subestipulante_~",
                        column: x => x.apolice_subestipulante_id,
                        principalSchema: "seguro",
                        principalTable: "apolice_subestipulante",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_proposta_apolice",
                schema: "seguro",
                table: "proposta",
                column: "apolice_id",
                filter: "apolice_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_apolice_vida",
                schema: "seguro",
                table: "proposta",
                column: "apolice_vida_id",
                filter: "apolice_vida_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_apolice_apolice_origem_id",
                schema: "seguro",
                table: "apolice",
                column: "apolice_origem_id");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_estipulante",
                schema: "seguro",
                table: "apolice",
                column: "estipulante_id");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_seguradora",
                schema: "seguro",
                table: "apolice",
                column: "seguradora_id");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_status",
                schema: "seguro",
                table: "apolice",
                column: "status",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_vigencia",
                schema: "seguro",
                table: "apolice",
                columns: new[] { "data_inicio_vigencia", "data_fim_vigencia" });

            migrationBuilder.CreateIndex(
                name: "ux_apolice_legado",
                schema: "seguro",
                table: "apolice",
                column: "legado_id",
                unique: true,
                filter: "legado_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_ramo_apolice",
                schema: "seguro",
                table: "apolice_ramo",
                column: "apolice_id");

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

            migrationBuilder.CreateIndex(
                name: "ix_apolice_sub_apolice",
                schema: "seguro",
                table: "apolice_subestipulante",
                column: "apolice_id");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_sub_subestipulante",
                schema: "seguro",
                table: "apolice_subestipulante",
                column: "subestipulante_id");

            migrationBuilder.CreateIndex(
                name: "ux_apolice_sub_ativo",
                schema: "seguro",
                table: "apolice_subestipulante",
                columns: new[] { "apolice_id", "subestipulante_id" },
                unique: true,
                filter: "ativo = true AND deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_vida_apolice",
                schema: "seguro",
                table: "apolice_vida",
                column: "apolice_id");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_vida_cliente",
                schema: "seguro",
                table: "apolice_vida",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_vida_status",
                schema: "seguro",
                table: "apolice_vida",
                column: "status",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_vida_subestip",
                schema: "seguro",
                table: "apolice_vida",
                column: "apolice_subestipulante_id",
                filter: "apolice_subestipulante_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_apolice_vida_vigencia",
                schema: "seguro",
                table: "apolice_vida",
                columns: new[] { "data_inicio_vigencia", "data_fim_vigencia" });

            migrationBuilder.CreateIndex(
                name: "ux_apolice_vida_legado",
                schema: "seguro",
                table: "apolice_vida",
                column: "legado_id",
                unique: true,
                filter: "legado_id IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "proposta_apolice_id_fkey",
                schema: "seguro",
                table: "proposta",
                column: "apolice_id",
                principalSchema: "seguro",
                principalTable: "apolice",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "proposta_apolice_vida_id_fkey",
                schema: "seguro",
                table: "proposta",
                column: "apolice_vida_id",
                principalSchema: "seguro",
                principalTable: "apolice_vida",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "proposta_apolice_id_fkey",
                schema: "seguro",
                table: "proposta");

            migrationBuilder.DropForeignKey(
                name: "proposta_apolice_vida_id_fkey",
                schema: "seguro",
                table: "proposta");

            migrationBuilder.DropTable(
                name: "apolice_ramo",
                schema: "seguro");

            migrationBuilder.DropTable(
                name: "apolice_vida",
                schema: "seguro");

            migrationBuilder.DropTable(
                name: "apolice_subestipulante",
                schema: "seguro");

            migrationBuilder.DropTable(
                name: "apolice",
                schema: "seguro");

            migrationBuilder.DropIndex(
                name: "ix_proposta_apolice",
                schema: "seguro",
                table: "proposta");

            migrationBuilder.DropIndex(
                name: "ix_proposta_apolice_vida",
                schema: "seguro",
                table: "proposta");

            migrationBuilder.DropColumn(
                name: "apolice_id",
                schema: "seguro",
                table: "proposta");

            migrationBuilder.DropColumn(
                name: "apolice_vida_id",
                schema: "seguro",
                table: "proposta");
        }
    }
}
