using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CicloFinanceiroPropostas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "apolice_vida_id",
                schema: "seguro",
                table: "proposta_movimento",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "parcela_competencia_id",
                schema: "seguro",
                table: "proposta_movimento",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "proposta_financeiro_id",
                schema: "seguro",
                table: "proposta_movimento",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "revisao_baixa",
                schema: "seguro",
                table: "proposta_movimento",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "parcela_competencia",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false),
                    apolice_id = table.Column<long>(type: "bigint", nullable: false),
                    competencia = table.Column<int>(type: "integer", nullable: false),
                    gerada_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fechada_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revisao = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parcela_competencia", x => x.id);
                    table.CheckConstraint("ck_parcela_competencia_mes", "competencia % 100 BETWEEN 1 AND 12 AND competencia BETWEEN 101 AND 999912 AND revisao >= 0 AND (fechada_em IS NULL OR gerada_em IS NOT NULL)");
                    table.ForeignKey(
                        name: "fk_parcela_competencia_apolices_apolice_id",
                        column: x => x.apolice_id,
                        principalSchema: "seguro",
                        principalTable: "apolice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "proposta_financeiro",
                schema: "seguro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false),
                    proposta_id = table.Column<long>(type: "bigint", nullable: false),
                    apolice_ramo_id = table.Column<long>(type: "bigint", nullable: false),
                    inicio_conferido = table.Column<DateOnly>(type: "date", nullable: false),
                    competencia_inicial = table.Column<int>(type: "integer", nullable: false),
                    vencimento_inicial = table.Column<DateOnly>(type: "date", nullable: false),
                    competencia_referencia = table.Column<int>(type: "integer", nullable: false),
                    vencimento_referencia = table.Column<DateOnly>(type: "date", nullable: false),
                    premio_liquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    iof_percentual = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    iof_valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_mensal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    condicoes_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    conferida_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_proposta_financeiro", x => x.id);
                    table.CheckConstraint("ck_proposta_financeiro_valores", "premio_liquido >= 0 AND iof_percentual BETWEEN 0 AND 100 AND iof_valor >= 0 AND total_mensal = premio_liquido + iof_valor AND competencia_inicial % 100 BETWEEN 1 AND 12 AND competencia_inicial BETWEEN 101 AND 999912 AND competencia_referencia % 100 BETWEEN 1 AND 12 AND competencia_referencia BETWEEN 101 AND 999912");
                    table.ForeignKey(
                        name: "fk_proposta_financeiro_apolice_ramos_apolice_ramo_id",
                        column: x => x.apolice_ramo_id,
                        principalSchema: "seguro",
                        principalTable: "apolice_ramo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_proposta_financeiro_proposta_proposta_id",
                        column: x => x.proposta_id,
                        principalSchema: "seguro",
                        principalTable: "proposta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_proposta_movimento_parcela_competencia_id",
                schema: "seguro",
                table: "proposta_movimento",
                column: "parcela_competencia_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_movimento_proposta_financeiro_id",
                schema: "seguro",
                table: "proposta_movimento",
                column: "proposta_financeiro_id");

            migrationBuilder.CreateIndex(
                name: "ux_parcela_vida_competencia",
                schema: "seguro",
                table: "proposta_movimento",
                columns: new[] { "apolice_vida_id", "competencia_int" },
                unique: true,
                filter: "legado_id IS NULL AND classificacao = 'PARCELA_MENSAL'");

            migrationBuilder.CreateIndex(
                name: "ux_parcela_vida_numero",
                schema: "seguro",
                table: "proposta_movimento",
                columns: new[] { "apolice_vida_id", "parcela" },
                unique: true,
                filter: "legado_id IS NULL AND classificacao = 'PARCELA_MENSAL'");

            migrationBuilder.AddCheckConstraint(
                name: "ck_parcela_mensal_nativa",
                schema: "seguro",
                table: "proposta_movimento",
                sql: "legado_id IS NOT NULL OR classificacao <> 'PARCELA_MENSAL' OR (public_id IS NOT NULL AND proposta_id IS NOT NULL AND apolice_vida_id IS NOT NULL AND proposta_financeiro_id IS NOT NULL AND parcela_competencia_id IS NOT NULL AND parcela IS NOT NULL AND parcela > 0 AND competencia_int IS NOT NULL AND competencia_int BETWEEN 101 AND 999912 AND competencia_int % 100 BETWEEN 1 AND 12 AND data_vencimento IS NOT NULL AND premio_liquido >= 0 AND iof >= 0 AND premio_total = premio_liquido + iof AND premio_total IS NOT NULL AND premio_liquido IS NOT NULL AND iof IS NOT NULL AND (valor_pago IS NULL OR valor_pago = 0 OR valor_pago = premio_total) AND revisao_baixa >= 0)");

            migrationBuilder.CreateIndex(
                name: "ix_parcela_competencia_apolice_id_competencia",
                schema: "seguro",
                table: "parcela_competencia",
                columns: new[] { "apolice_id", "competencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_parcela_competencia_public_id",
                schema: "seguro",
                table: "parcela_competencia",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_proposta_financeiro_apolice_ramo_id",
                schema: "seguro",
                table: "proposta_financeiro",
                column: "apolice_ramo_id");

            migrationBuilder.CreateIndex(
                name: "ix_proposta_financeiro_proposta_id",
                schema: "seguro",
                table: "proposta_financeiro",
                column: "proposta_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_proposta_financeiro_public_id",
                schema: "seguro",
                table: "proposta_financeiro",
                column: "public_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_proposta_movimento_apolice_vidas_apolice_vida_id",
                schema: "seguro",
                table: "proposta_movimento",
                column: "apolice_vida_id",
                principalSchema: "seguro",
                principalTable: "apolice_vida",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_proposta_movimento_parcelas_competencias_parcela_competenci",
                schema: "seguro",
                table: "proposta_movimento",
                column: "parcela_competencia_id",
                principalSchema: "seguro",
                principalTable: "parcela_competencia",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_proposta_movimento_propostas_financeiro_proposta_financeiro",
                schema: "seguro",
                table: "proposta_movimento",
                column: "proposta_financeiro_id",
                principalSchema: "seguro",
                principalTable: "proposta_financeiro",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$ BEGIN
                  IF EXISTS (SELECT 1 FROM seguro.proposta_financeiro) OR EXISTS (SELECT 1 FROM seguro.parcela_competencia)
                     OR EXISTS (SELECT 1 FROM seguro.proposta_movimento WHERE public_id IS NOT NULL OR legado_id IS NULL OR proposta_financeiro_id IS NOT NULL OR parcela_competencia_id IS NOT NULL OR apolice_vida_id IS NOT NULL OR revisao_baixa <> 0)
                  THEN RAISE EXCEPTION 'Rollback bloqueado: há dados financeiros nativos a preservar'; END IF;
                END $$;
                """);
            migrationBuilder.DropForeignKey(
                name: "fk_proposta_movimento_apolice_vidas_apolice_vida_id",
                schema: "seguro",
                table: "proposta_movimento");

            migrationBuilder.DropForeignKey(
                name: "fk_proposta_movimento_parcelas_competencias_parcela_competenci",
                schema: "seguro",
                table: "proposta_movimento");

            migrationBuilder.DropForeignKey(
                name: "fk_proposta_movimento_propostas_financeiro_proposta_financeiro",
                schema: "seguro",
                table: "proposta_movimento");

            migrationBuilder.DropTable(
                name: "parcela_competencia",
                schema: "seguro");

            migrationBuilder.DropTable(
                name: "proposta_financeiro",
                schema: "seguro");

            migrationBuilder.DropIndex(
                name: "ix_proposta_movimento_parcela_competencia_id",
                schema: "seguro",
                table: "proposta_movimento");

            migrationBuilder.DropIndex(
                name: "ix_proposta_movimento_proposta_financeiro_id",
                schema: "seguro",
                table: "proposta_movimento");

            migrationBuilder.DropIndex(
                name: "ux_parcela_vida_competencia",
                schema: "seguro",
                table: "proposta_movimento");

            migrationBuilder.DropIndex(
                name: "ux_parcela_vida_numero",
                schema: "seguro",
                table: "proposta_movimento");

            migrationBuilder.DropCheckConstraint(
                name: "ck_parcela_mensal_nativa",
                schema: "seguro",
                table: "proposta_movimento");

            migrationBuilder.DropColumn(
                name: "apolice_vida_id",
                schema: "seguro",
                table: "proposta_movimento");

            migrationBuilder.DropColumn(
                name: "parcela_competencia_id",
                schema: "seguro",
                table: "proposta_movimento");

            migrationBuilder.DropColumn(
                name: "proposta_financeiro_id",
                schema: "seguro",
                table: "proposta_movimento");

            migrationBuilder.DropColumn(
                name: "revisao_baixa",
                schema: "seguro",
                table: "proposta_movimento");
        }
    }
}
