using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Seguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarParametrizacaoApolice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "apolice_configuracao",
                schema: "seguro",
                columns: table => new
                {
                    apolice_id = table.Column<long>(type: "bigint", nullable: false),
                    tipo_adesao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    custeio = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    carencia_dias = table.Column<int>(type: "integer", nullable: true),
                    mes_base_reajuste = table.Column<int>(type: "integer", nullable: true),
                    indice_reajuste = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cobre_conjuge = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    controla_excedente = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    dia_corte_faturamento = table.Column<int>(type: "integer", nullable: true),
                    prazo_aviso_sinistro_dias = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_apolice_configuracao", x => x.apolice_id);
                    table.ForeignKey(
                        name: "fk_apolice_configuracao_apolice_apolice_id",
                        column: x => x.apolice_id,
                        principalSchema: "seguro",
                        principalTable: "apolice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "apolice_historico",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    apolice_id = table.Column<long>(type: "bigint", nullable: false),
                    acao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    usuario_public_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_acao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_apolice_historico", x => x.id);
                    table.ForeignKey(
                        name: "fk_apolice_historico_apolice_apolice_id",
                        column: x => x.apolice_id,
                        principalSchema: "seguro",
                        principalTable: "apolice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_apolice_historico_apolice_id",
                schema: "seguro",
                table: "apolice_historico",
                column: "apolice_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "apolice_configuracao",
                schema: "seguro");

            migrationBuilder.DropTable(
                name: "apolice_historico",
                schema: "seguro");
        }
    }
}
