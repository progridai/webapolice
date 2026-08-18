using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Atendimento.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "atendimento");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.CreateTable(
                name: "protocolo_lote",
                schema: "atendimento",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    numero_protocolo = table.Column<int>(type: "integer", nullable: true),
                    data_protocolo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    consultor_legado_id = table.Column<int>(type: "integer", nullable: true),
                    usuario_legado_id = table.Column<int>(type: "integer", nullable: true),
                    anexo_consultor = table.Column<bool>(type: "boolean", nullable: true),
                    anexo_seguradora = table.Column<bool>(type: "boolean", nullable: true),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValueSql: "'ativo'::character varying"),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("protocolo_lote_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "protocolo_relatorio_seguradora",
                schema: "atendimento",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data_relatorio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("protocolo_relatorio_seguradora_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "protocolo_acompanhamento",
                schema: "atendimento",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    protocolo_lote_id = table.Column<long>(type: "bigint", nullable: true),
                    data_acompanhamento = table.Column<DateOnly>(type: "date", nullable: true),
                    hora_original = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    contato = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    usuario_legado_id = table.Column<int>(type: "integer", nullable: true),
                    legado_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("protocolo_acompanhamento_pkey", x => x.id);
                    table.ForeignKey(
                        name: "protocolo_acompanhamento_protocolo_lote_id_fkey",
                        column: x => x.protocolo_lote_id,
                        principalSchema: "atendimento",
                        principalTable: "protocolo_lote",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "protocolo_item",
                schema: "atendimento",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    protocolo_lote_id = table.Column<long>(type: "bigint", nullable: false),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    cliente_id = table.Column<long>(type: "bigint", nullable: true),
                    cliente_vinculo_id = table.Column<long>(type: "bigint", nullable: true),
                    estipulante_id = table.Column<long>(type: "bigint", nullable: true),
                    premio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    data_vigencia = table.Column<DateOnly>(type: "date", nullable: true),
                    equipe = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    matricula = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    tipo_item = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValueSql: "'titular'::character varying"),
                    nome_conjuge = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    origem_legado = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    legado_id = table.Column<int>(type: "integer", nullable: false),
                    legado_cliente_id = table.Column<int>(type: "integer", nullable: true),
                    legado_estipulante_id = table.Column<int>(type: "integer", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("protocolo_item_pkey", x => x.id);
                    table.ForeignKey(
                        name: "protocolo_item_protocolo_lote_id_fkey",
                        column: x => x.protocolo_lote_id,
                        principalSchema: "atendimento",
                        principalTable: "protocolo_lote",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "protocolo_relatorio_seguradora_item",
                schema: "atendimento",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    relatorio_id = table.Column<long>(type: "bigint", nullable: false),
                    protocolo_lote_id = table.Column<long>(type: "bigint", nullable: true),
                    pessoa_id = table.Column<long>(type: "bigint", nullable: true),
                    cliente_id = table.Column<long>(type: "bigint", nullable: true),
                    cliente_vinculo_id = table.Column<long>(type: "bigint", nullable: true),
                    legado_cliente_id = table.Column<int>(type: "integer", nullable: true),
                    legado_protocolo_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("protocolo_relatorio_seguradora_item_pkey", x => x.id);
                    table.ForeignKey(
                        name: "protocolo_relatorio_seguradora_item_protocolo_lote_id_fkey",
                        column: x => x.protocolo_lote_id,
                        principalSchema: "atendimento",
                        principalTable: "protocolo_lote",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "protocolo_relatorio_seguradora_item_relatorio_id_fkey",
                        column: x => x.relatorio_id,
                        principalSchema: "atendimento",
                        principalTable: "protocolo_relatorio_seguradora",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_acompanhamento_data",
                schema: "atendimento",
                table: "protocolo_acompanhamento",
                column: "data_acompanhamento");

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_acompanhamento_lote",
                schema: "atendimento",
                table: "protocolo_acompanhamento",
                column: "protocolo_lote_id");

            migrationBuilder.CreateIndex(
                name: "ux_protocolo_acompanhamento_legado",
                schema: "atendimento",
                table: "protocolo_acompanhamento",
                column: "legado_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_item_cliente",
                schema: "atendimento",
                table: "protocolo_item",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_item_estipulante",
                schema: "atendimento",
                table: "protocolo_item",
                column: "estipulante_id");

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_item_lote",
                schema: "atendimento",
                table: "protocolo_item",
                column: "protocolo_lote_id");

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_item_matricula",
                schema: "atendimento",
                table: "protocolo_item",
                column: "matricula");

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_item_tipo",
                schema: "atendimento",
                table: "protocolo_item",
                column: "tipo_item");

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_item_vinculo",
                schema: "atendimento",
                table: "protocolo_item",
                column: "cliente_vinculo_id");

            migrationBuilder.CreateIndex(
                name: "ux_protocolo_item_legado_origem",
                schema: "atendimento",
                table: "protocolo_item",
                columns: new[] { "origem_legado", "legado_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_lote_consultor",
                schema: "atendimento",
                table: "protocolo_lote",
                column: "consultor_legado_id");

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_lote_data",
                schema: "atendimento",
                table: "protocolo_lote",
                column: "data_protocolo");

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_lote_numero",
                schema: "atendimento",
                table: "protocolo_lote",
                column: "numero_protocolo");

            migrationBuilder.CreateIndex(
                name: "ux_protocolo_lote_legado",
                schema: "atendimento",
                table: "protocolo_lote",
                column: "legado_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_rel_seg_data",
                schema: "atendimento",
                table: "protocolo_relatorio_seguradora",
                column: "data_relatorio");

            migrationBuilder.CreateIndex(
                name: "ux_protocolo_rel_seg_legado",
                schema: "atendimento",
                table: "protocolo_relatorio_seguradora",
                column: "legado_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_rel_seg_item_cliente",
                schema: "atendimento",
                table: "protocolo_relatorio_seguradora_item",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_rel_seg_item_protocolo",
                schema: "atendimento",
                table: "protocolo_relatorio_seguradora_item",
                column: "protocolo_lote_id");

            migrationBuilder.CreateIndex(
                name: "ix_protocolo_rel_seg_item_relatorio",
                schema: "atendimento",
                table: "protocolo_relatorio_seguradora_item",
                column: "relatorio_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "protocolo_acompanhamento",
                schema: "atendimento");

            migrationBuilder.DropTable(
                name: "protocolo_item",
                schema: "atendimento");

            migrationBuilder.DropTable(
                name: "protocolo_relatorio_seguradora_item",
                schema: "atendimento");

            migrationBuilder.DropTable(
                name: "protocolo_lote",
                schema: "atendimento");

            migrationBuilder.DropTable(
                name: "protocolo_relatorio_seguradora",
                schema: "atendimento");
        }
    }
}
